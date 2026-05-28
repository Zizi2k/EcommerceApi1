(function () {
    var STORAGE_KEY = 'store-theme';

    function ensureAppNotify() {
        if (window.AppNotify) return;

        var styleId = 'app-notify-style';
        if (!document.getElementById(styleId)) {
            var style = document.createElement('style');
            style.id = styleId;
            style.textContent = '.app-notify-wrap{position:fixed;top:20px;right:20px;z-index:10000;display:flex;flex-direction:column;gap:10px;pointer-events:none}.app-notify{min-width:280px;max-width:420px;padding:12px 14px;border-radius:12px;display:flex;align-items:flex-start;gap:10px;font-family:Poppins,sans-serif;box-shadow:0 12px 30px rgba(15,23,42,.18);border:1px solid transparent;background:#fff;color:#0f172a;opacity:0;transform:translateY(-8px);transition:opacity .2s ease,transform .2s ease}.app-notify.show{opacity:1;transform:translateY(0)}.app-notify i{margin-top:2px}.app-notify__title{font-size:14px;font-weight:700;line-height:1.25}.app-notify__text{font-size:13px;line-height:1.4;margin-top:2px}.app-notify--success{background:linear-gradient(135deg,#ecfdf5 0%,#f0fdf4 100%);border-color:#86efac;color:#14532d}.app-notify--success i{color:#16a34a}@media (max-width:560px){.app-notify-wrap{left:12px;right:12px;top:12px}.app-notify{min-width:0;max-width:none}}';
            document.head.appendChild(style);
        }

        function getWrap() {
            var wrap = document.getElementById('app-notify-wrap');
            if (!wrap) {
                wrap = document.createElement('div');
                wrap.id = 'app-notify-wrap';
                wrap.className = 'app-notify-wrap';
                document.body.appendChild(wrap);
            }
            return wrap;
        }

        window.AppNotify = {
            success: function (message, timeoutMs) {
                var card = document.createElement('div');
                card.className = 'app-notify app-notify--success';
                card.innerHTML = '<i class="fa-solid fa-circle-check" aria-hidden="true"></i>' +
                    '<div><div class="app-notify__title">Thành công</div><div class="app-notify__text">' +
                    (message || 'Thao tác đã hoàn tất.') + '</div></div>';
                getWrap().appendChild(card);
                requestAnimationFrame(function () { card.classList.add('show'); });
                setTimeout(function () {
                    card.classList.remove('show');
                    setTimeout(function () {
                        if (card.parentNode) card.parentNode.removeChild(card);
                    }, 220);
                }, typeof timeoutMs === 'number' ? timeoutMs : 2400);
            }
        };
    }

    function getSystemTheme() {
        return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches
            ? 'dark' : 'light';
    }

    function getStoredTheme() {
        var t = localStorage.getItem(STORAGE_KEY);
        return t === 'dark' || t === 'light' ? t : null;
    }

    function getTheme() {
        return getStoredTheme() || getSystemTheme();
    }

    function applyTheme(theme) {
        var t = theme === 'dark' ? 'dark' : 'light';
        var prev = document.documentElement.getAttribute('data-theme') || '';
        document.documentElement.setAttribute('data-theme', t);
        localStorage.setItem(STORAGE_KEY, t);
        updateToggleUi(t);
        window.dispatchEvent(new CustomEvent('store-theme-changed', {
            detail: { theme: t, previousTheme: prev }
        }));
    }

    function updateToggleUi(theme) {
        var btn = document.getElementById('theme-toggle');
        if (!btn) return;
        var isDark = theme === 'dark';
        btn.setAttribute('aria-label', isDark ? 'Chuyển giao diện sáng' : 'Chuyển giao diện tối');
        btn.setAttribute('title', isDark ? 'Giao diện sáng' : 'Giao diện tối');
        var icon = btn.querySelector('i');
        if (icon) {
            icon.className = isDark ? 'fa-solid fa-sun' : 'fa-solid fa-moon';
        }
        btn.classList.toggle('is-dark', isDark);
        var label = btn.querySelector('.theme-toggle-label');
        if (label) label.textContent = isDark ? 'Sáng' : 'Tối';
    }

    function toggleTheme() {
        var next = getTheme() === 'dark' ? 'light' : 'dark';
        applyTheme(next);
    }

    function injectThemeToggle() {
        if (document.getElementById('theme-toggle')) return;

        var btn = document.createElement('button');
        btn.type = 'button';
        btn.id = 'theme-toggle';
        btn.className = 'theme-toggle-btn';
        btn.innerHTML = '<i class="fa-solid fa-moon" aria-hidden="true"></i><span class="theme-toggle-label"></span>';

        btn.addEventListener('click', function (e) {
            e.stopPropagation();
            toggleTheme();
        });

        var headerRight = document.querySelector('.header-right');
        if (headerRight) {
            headerRight.insertBefore(btn, headerRight.firstChild);
        } else {
            btn.classList.add('theme-toggle-btn--floating');
            document.body.appendChild(btn);
        }

        updateToggleUi(getTheme());
    }

    window.getStoreTheme = getTheme;
    window.applyStoreTheme = applyTheme;
    window.toggleStoreTheme = toggleTheme;

    ensureAppNotify();
    applyTheme(getStoredTheme() || getSystemTheme());

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', injectThemeToggle);
    } else {
        injectThemeToggle();
    }
})();
