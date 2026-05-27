(function () {
    var STORAGE_KEY = 'store-theme';

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
        document.documentElement.setAttribute('data-theme', t);
        localStorage.setItem(STORAGE_KEY, t);
        updateToggleUi(t);
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

    applyTheme(getStoredTheme() || getSystemTheme());

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', injectThemeToggle);
    } else {
        injectThemeToggle();
    }
})();
