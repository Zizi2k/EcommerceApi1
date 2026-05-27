(function () {
    var pollTimer = null;
    var moduleLoaded = false;

    function ensureStyles() {
        if (document.getElementById('notif-styles-link')) return;
        var link = document.createElement('link');
        link.id = 'notif-styles-link';
        link.rel = 'stylesheet';
        link.href = 'css/notifications.css';
        document.head.appendChild(link);
    }

    function ensureUi() {
        var headerRight = document.querySelector('.header-right');
        if (!headerRight || document.getElementById('notif-menu')) return;

        var wrap = document.createElement('div');
        wrap.className = 'notif-menu';
        wrap.id = 'notif-menu';
        wrap.style.display = 'none';
        wrap.innerHTML =
            '<button type="button" class="notif-btn" id="notif-btn" title="Thông báo" aria-label="Thông báo">' +
            '<i class="fa-solid fa-bell"></i>' +
            '<span class="notif-badge" id="notif-badge" style="display:none;">0</span>' +
            '</button>' +
            '<div class="notif-dropdown" id="notif-dropdown" role="dialog" aria-label="Danh sách thông báo">' +
            '<div class="notif-dropdown-head">' +
            '<strong>Thông báo</strong>' +
            '<button type="button" class="notif-mark-all" id="notif-mark-all">Đánh dấu đã đọc</button>' +
            '</div>' +
            '<div class="notif-list" id="notif-list">' +
            '<div class="notif-loading">Đang tải...</div>' +
            '</div>' +
            '<div class="notif-dropdown-foot">Nhấn thông báo để mở trang liên quan</div>' +
            '</div>';

        var cartBtn = headerRight.querySelector('.cart-icon');
        if (cartBtn) {
            headerRight.insertBefore(wrap, cartBtn);
        } else {
            headerRight.insertBefore(wrap, headerRight.firstChild);
        }

        var btn = document.getElementById('notif-btn');
        var dropdown = document.getElementById('notif-dropdown');
        var markAll = document.getElementById('notif-mark-all');

        if (btn && dropdown) {
            btn.addEventListener('click', function (e) {
                e.stopPropagation();
                var open = dropdown.classList.toggle('show');
                if (open) loadNotificationsList();
            });
        }

        if (markAll) {
            markAll.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                markAllNotificationsRead();
            });
        }

        document.addEventListener('click', function (e) {
            if (!dropdown || !dropdown.classList.contains('show')) return;
            var menu = document.getElementById('notif-menu');
            if (menu && !menu.contains(e.target)) {
                dropdown.classList.remove('show');
            }
        });
    }

    function fmtTime(iso) {
        if (!iso) return '';
        try {
            return new Date(iso).toLocaleString('vi-VN', { dateStyle: 'short', timeStyle: 'short' });
        } catch (e) {
            return iso;
        }
    }

    function updateBadge(count) {
        var badge = document.getElementById('notif-badge');
        if (!badge) return;
        var n = Number(count) || 0;
        if (n > 0) {
            badge.style.display = 'block';
            badge.textContent = n > 99 ? '99+' : String(n);
        } else {
            badge.style.display = 'none';
            badge.textContent = '0';
        }
    }

    async function refreshUnreadCount() {
        if (!getToken()) return;
        try {
            var data = await apiGet(API_ENDPOINTS.NOTIFICATIONS_UNREAD);
            updateBadge(data && data.count != null ? data.count : 0);
        } catch (e) {
            /* ignore */
        }
    }

    async function loadNotificationsList() {
        var list = document.getElementById('notif-list');
        if (!list || !getToken()) return;
        list.innerHTML = '<div class="notif-loading">Đang tải...</div>';
        try {
            var items = await apiGet(API_ENDPOINTS.NOTIFICATIONS + '?limit=30');
            if (!Array.isArray(items) || !items.length) {
                list.innerHTML = '<div class="notif-empty">Chưa có thông báo.</div>';
                return;
            }
            list.innerHTML = items.map(function (n) {
                var cls = 'notif-item' + (n.isRead ? '' : ' is-unread');
                return '<button type="button" class="' + cls + '" data-id="' + n.id + '" data-link="' +
                    (n.linkUrl || '').replace(/"/g, '&quot;') + '">' +
                    '<span class="notif-item-title">' + esc(n.title) + '</span>' +
                    '<span class="notif-item-msg">' + esc(n.message) + '</span>' +
                    '<span class="notif-item-time">' + fmtTime(n.createdAtUtc) + '</span>' +
                    '</button>';
            }).join('');

            list.querySelectorAll('.notif-item').forEach(function (btn) {
                btn.addEventListener('click', function () {
                    var id = parseInt(btn.getAttribute('data-id'), 10);
                    var link = btn.getAttribute('data-link') || '';
                    openNotification(id, link);
                });
            });
        } catch (e) {
            list.innerHTML = '<div class="notif-empty">Không tải được thông báo.</div>';
        }
    }

    function esc(s) {
        return String(s == null ? '' : s)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/"/g, '&quot;');
    }

    async function openNotification(id, link) {
        try {
            await apiPut(API_ENDPOINTS.NOTIFICATIONS + '/' + id + '/read', {});
        } catch (e) { /* ignore */ }
        var dropdown = document.getElementById('notif-dropdown');
        if (dropdown) dropdown.classList.remove('show');
        await refreshUnreadCount();
        if (link) {
            if (link.indexOf('admin.html') >= 0 && typeof refreshSelectedOrderFromApi === 'function') {
                setTimeout(refreshSelectedOrderFromApi, 400);
            }
            window.location.href = link;
        }
    }

    async function markAllNotificationsRead() {
        try {
            await apiPut(API_ENDPOINTS.NOTIFICATIONS_READ_ALL, {});
            await refreshUnreadCount();
            await loadNotificationsList();
        } catch (e) {
            /* ignore */
        }
    }

    function startPolling() {
        stopPolling();
        refreshUnreadCount();
        pollTimer = setInterval(refreshUnreadCount, 45000);
    }

    function stopPolling() {
        if (pollTimer) {
            clearInterval(pollTimer);
            pollTimer = null;
        }
    }

    window.initNotifications = function () {
        if (moduleLoaded) return;
        moduleLoaded = true;
        ensureStyles();
        ensureUi();

        var menu = document.getElementById('notif-menu');
        if (!getToken()) {
            if (menu) menu.style.display = 'none';
            stopPolling();
            updateBadge(0);
            return;
        }
        if (menu) menu.style.display = 'inline-flex';
        startPolling();
    };

    window.refreshNotifications = refreshUnreadCount;
})();
