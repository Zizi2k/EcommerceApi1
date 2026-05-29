(function () {
    var SLOT_TIMES = [
        { key: '09:00-12:00', start: 9 * 60, end: 12 * 60 },
        { key: '12:00-15:00', start: 12 * 60, end: 15 * 60 },
        { key: '15:00-17:00', start: 15 * 60, end: 17 * 60 },
        { key: '17:00-19:00', start: 17 * 60, end: 19 * 60 },
        { key: '19:00-21:00', start: 19 * 60, end: 21 * 60 },
        { key: '21:00-23:00', start: 21 * 60, end: 23 * 60 }
    ];

    function escHtml(s) {
        return String(s == null ? '' : s)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/"/g, '&quot;');
    }

    function formatPrice(v) {
        return Number(v || 0).toLocaleString('vi-VN') + ' VNĐ';
    }

    function calcDiscountPercent(original, current) {
        var orig = Number(original);
        var curr = Number(current);
        if (!orig || !curr || curr >= orig) return 0;
        return Math.round(((orig - curr) / orig) * 100);
    }

    function getVietnamNow() {
        var now = new Date();
        return new Date(now.getTime() + 7 * 3600000 + now.getTimezoneOffset() * 60000);
    }

    function formatCountdown(ms) {
        if (ms <= 0) return 'Đang diễn ra';
        var totalSec = Math.floor(ms / 1000);
        var h = Math.floor(totalSec / 3600);
        var m = Math.floor((totalSec % 3600) / 60);
        var s = totalSec % 60;
        return 'Kết thúc trong ' + String(h).padStart(2, '0') + ':' + String(m).padStart(2, '0') + ':' + String(s).padStart(2, '0');
    }

    function updateTimebar() {
        var now = getVietnamNow();
        var minute = now.getHours() * 60 + now.getMinutes();
        var active = null;

        document.querySelectorAll('.flash-slot').forEach(function (btn) {
            var slotKey = btn.getAttribute('data-slot');
            var slot = SLOT_TIMES.find(function (s) { return s.key === slotKey; });
            if (!slot) return;
            var state = 'Sắp diễn ra';
            if (minute >= slot.start && minute < slot.end) {
                state = 'Đang diễn ra';
                active = slot;
                btn.classList.add('is-active');
            } else {
                btn.classList.remove('is-active');
                if (minute >= slot.end) state = 'Đã kết thúc';
            }
            var cap = btn.querySelector('span');
            if (cap) cap.textContent = state;
        });

        var countdown = document.getElementById('flash-sale-countdown');
        if (countdown) {
            if (!active) {
                countdown.textContent = 'Đang chờ khung giờ sale';
            } else {
                var endMs = new Date(now.getFullYear(), now.getMonth(), now.getDate(), Math.floor(active.end / 60), active.end % 60, 0, 0).getTime();
                countdown.textContent = formatCountdown(endMs - now.getTime());
            }
        }
    }

    function renderItems(targetId, items) {
        var el = document.getElementById(targetId);
        if (!el) return;
        if (!items || !items.length) {
            el.innerHTML = '<div class="flash-sale-empty">Chưa có sản phẩm trong nhóm này.</div>';
            return;
        }
        el.innerHTML = items.map(function (p) {
            var href = p.productId ? 'product.html?id=' + encodeURIComponent(p.productId) : '#products';
            var discount = calcDiscountPercent(p.price, p.displayPrice);
            var badge = discount > 0
                ? '<span class="flash-sale-item__badge">-' + discount + '%</span>'
                : '';
            var old = p.price && p.displayPrice && Number(p.displayPrice) < Number(p.price)
                ? '<span class="flash-sale-item__old">' + escHtml(formatPrice(p.price)) + '</span>'
                : '';
            var addBtn = p.productId
                ? '<button type="button" class="flash-sale-item__add" data-product-id="' + Number(p.productId) + '">' +
                  '<i class="fa-solid fa-cart-plus"></i> Thêm vào giỏ</button>'
                : '';
            return '<article class="flash-sale-item">' +
                '<a class="flash-sale-item__link" href="' + href + '">' +
                '<div class="flash-sale-item__media">' + badge +
                '<img src="' + escHtml(p.imageUrl || '') + '" alt="" onerror="this.src=\'https://via.placeholder.com/64\'">' +
                '</div>' +
                '<div class="flash-sale-item__name">' + escHtml(p.productName || p.headline || 'Sản phẩm') + '</div>' +
                '<div class="flash-sale-item__price"><span class="flash-sale-item__now">' + escHtml(formatPrice(p.displayPrice)) + '</span>' + old + '</div>' +
                '</a>' + addBtn +
                '</article>';
        }).join('');

        el.querySelectorAll('.flash-sale-item__add').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var id = parseInt(btn.getAttribute('data-product-id'), 10);
                if (!isNaN(id)) addFlashSaleToCart(id, btn);
            });
        });
    }

    async function addFlashSaleToCart(productId, button) {
        var token = typeof getToken === 'function' ? getToken() : null;
        if (!token) {
            window.location.href = 'login.html';
            return;
        }
        if (button) button.disabled = true;
        try {
            await apiPost(API_ENDPOINTS.CART_ADD, { productId: productId, quantity: 1 });
            if (typeof updateCartCount === 'function') await updateCartCount();
            if (window.AppNotify && typeof window.AppNotify.success === 'function') {
                window.AppNotify.success('Đã thêm sản phẩm vào giỏ hàng.');
            }
        } catch (e) {
            alert(e.message || 'Không thêm được vào giỏ hàng.');
        } finally {
            if (button) button.disabled = false;
        }
    }

    async function loadFlashSale() {
        try {
            var data = await apiGet(API_ENDPOINTS.PROMOTIONS_FLASH_SALE);
            var daily = data && data.dailyProducts ? data.dailyProducts : [];
            var evt = data && data.eventProducts ? data.eventProducts : [];
            renderItems('flash-sale-all', daily.concat(evt));
        } catch (e) {
            renderItems('flash-sale-all', []);
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        loadFlashSale();
        updateTimebar();
        setInterval(updateTimebar, 1000);
    });
})();
