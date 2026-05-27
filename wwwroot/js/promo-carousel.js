(function () {
    var slides = [];
    var sortedPromos = [];
    var currentIndex = 0;
    var autoplayTimer = null;
    var AUTOPLAY_MS = 5500;

    var FALLBACK_SLIDES = [
        {
            headline: 'Công Nghệ Hiện Đại',
            subtitle: 'Khám phá sản phẩm công nghệ với giá tốt nhất',
            badgeText: 'MỚI',
            displayPrice: null,
            price: null,
            productId: null,
            imageUrl: 'https://images.unsplash.com/photo-1519389950473-47ba0277781c?q=80&w=800'
        }
    ];

    function escapeHtml(str) {
        return String(str == null ? '' : str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/"/g, '&quot;');
    }

    function formatPrice(n) {
        if (n == null || !Number.isFinite(Number(n))) return '';
        return Number(n).toLocaleString('vi-VN') + ' VNĐ';
    }

    function getDisplayPrice(raw) {
        if (raw.displayPrice != null) return Number(raw.displayPrice);
        if (raw.promoPrice != null) return Number(raw.promoPrice);
        if (raw.price != null) return Number(raw.price);
        return 0;
    }

    function normalizeSlide(raw) {
        var headline = (raw.headline && raw.headline.trim()) || raw.productName || 'Sản phẩm khuyến mãi';
        var subtitle = (raw.subtitle && raw.subtitle.trim()) || raw.description || '';
        var badge = (raw.badgeText && raw.badgeText.trim()) || 'SALE';
        var displayPrice = getDisplayPrice(raw);
        var original = raw.price != null ? Number(raw.price) : null;
        var showOld = original != null && displayPrice > 0 && displayPrice < original;

        return {
            id: raw.id,
            productId: raw.productId,
            headline: headline,
            subtitle: subtitle.slice(0, 120) + (subtitle.length > 120 ? '…' : ''),
            badgeText: badge,
            displayPrice: displayPrice,
            price: original,
            showOldPrice: showOld,
            imageUrl: raw.imageUrl || 'https://via.placeholder.com/400x400?text=Product'
        };
    }

    function sortByPriceHighToLow(list) {
        return list.slice().sort(function (a, b) {
            return (b.displayPrice || 0) - (a.displayPrice || 0);
        });
    }

    function renderSlideHtml(s, index) {
        var priceHtml = '';
        if (s.displayPrice > 0) {
            priceHtml = '<div class="promo-price-row">' +
                '<span class="promo-price-current">' + escapeHtml(formatPrice(s.displayPrice)) + '</span>';
            if (s.showOldPrice) {
                priceHtml += '<span class="promo-price-old">' + escapeHtml(formatPrice(s.price)) + '</span>';
            }
            priceHtml += '</div>';
        }

        var detailBtn = s.productId
            ? '<a class="promo-btn promo-btn-primary" href="product.html?id=' + encodeURIComponent(s.productId) + '">' +
              '<i class="fa-solid fa-eye"></i> Xem chi tiết</a>'
            : '';

        var shopBtn = '<a class="promo-btn promo-btn-ghost" href="#products">' +
            '<i class="fa-solid fa-bag-shopping"></i> Mua ngay</a>';

        var img = escapeHtml(s.imageUrl);
        var active = index === 0 ? ' is-active' : '';

        return '<article class="promo-slide' + active + '" data-index="' + index + '">' +
            '<div class="promo-slide-visual">' +
            '<img src="' + img + '" alt="' + escapeHtml(s.headline) + '" loading="lazy" decoding="async" ' +
            'onerror="this.src=\'https://via.placeholder.com/400x400?text=No+Image\'">' +
            '</div>' +
            '<div class="promo-slide-info">' +
            '<span class="promo-slide-badge">' + escapeHtml(s.badgeText) + '</span>' +
            '<h2>' + escapeHtml(s.headline) + '</h2>' +
            '<p>' + escapeHtml(s.subtitle) + '</p>' +
            priceHtml +
            '<div class="promo-slide-actions">' + detailBtn + shopBtn + '</div>' +
            '</div></article>';
    }

    function renderSaleItemHtml(s, rank) {
        var href = s.productId ? 'product.html?id=' + encodeURIComponent(s.productId) : '#products';
        var img = escapeHtml(s.imageUrl);
        var oldHtml = s.showOldPrice
            ? '<span class="promo-sale-item__old">' + escapeHtml(formatPrice(s.price)) + '</span>'
            : '';

        return '<a class="promo-sale-item" href="' + href + '">' +
            '<img class="promo-sale-item__img" src="' + img + '" alt="" loading="lazy" ' +
            'onerror="this.src=\'https://via.placeholder.com/72?text=+\'">' +
            '<div class="promo-sale-item__body">' +
            '<span class="promo-sale-item__badge">#' + rank + ' · ' + escapeHtml(s.badgeText) + '</span>' +
            '<div class="promo-sale-item__name">' + escapeHtml(s.headline) + '</div>' +
            '</div>' +
            '<div class="promo-sale-item__prices">' +
            '<span class="promo-sale-item__now">' + escapeHtml(formatPrice(s.displayPrice)) + '</span>' +
            oldHtml +
            '</div></a>';
    }

    function renderRankLists(list) {
        var topEl = document.getElementById('promo-list-top');
        var bottomEl = document.getElementById('promo-list-bottom');
        if (!topEl || !bottomEl) return;

        if (!list.length) {
            var empty = '<p class="promo-sale-empty">Chưa có sản phẩm sale</p>';
            topEl.innerHTML = empty;
            bottomEl.innerHTML = empty;
            return;
        }

        var mid = Math.ceil(list.length / 2);
        var topList = list.slice(0, mid);
        var bottomList = list.slice(mid);

        topEl.innerHTML = topList.map(function (s, i) {
            return renderSaleItemHtml(s, i + 1);
        }).join('') || '<p class="promo-sale-empty">—</p>';

        bottomEl.innerHTML = bottomList.map(function (s, i) {
            return renderSaleItemHtml(s, mid + i + 1);
        }).join('') || '<p class="promo-sale-empty">—</p>';
    }

    function buildDots(count) {
        var dots = document.getElementById('promo-carousel-dots');
        if (!dots) return;
        dots.innerHTML = '';
        for (var i = 0; i < count; i++) {
            var btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'promo-dot' + (i === 0 ? ' is-active' : '');
            btn.setAttribute('aria-label', 'Slide ' + (i + 1));
            btn.dataset.index = String(i);
            btn.addEventListener('click', function () {
                goToSlide(parseInt(this.dataset.index, 10));
                restartAutoplay();
            });
            dots.appendChild(btn);
        }
    }

    function goToSlide(index) {
        if (!slides.length) return;
        currentIndex = ((index % slides.length) + slides.length) % slides.length;
        document.querySelectorAll('.promo-slide').forEach(function (el, i) {
            el.classList.toggle('is-active', i === currentIndex);
        });
        document.querySelectorAll('.promo-dot').forEach(function (dot, i) {
            dot.classList.toggle('is-active', i === currentIndex);
        });
    }

    function nextSlide() { goToSlide(currentIndex + 1); }
    function prevSlide() { goToSlide(currentIndex - 1); }

    function restartAutoplay() {
        clearInterval(autoplayTimer);
        if (slides.length > 1) {
            autoplayTimer = setInterval(nextSlide, AUTOPLAY_MS);
        }
    }

    function mountCarousel(list) {
        slides = list;
        var inner = document.getElementById('promo-carousel-inner');
        if (!inner) return;

        inner.innerHTML = slides.map(renderSlideHtml).join('');
        buildDots(slides.length);

        var showNav = slides.length > 1;
        document.querySelectorAll('.promo-carousel-nav').forEach(function (btn) {
            btn.style.display = showNav ? '' : 'none';
        });
        var dotsWrap = document.getElementById('promo-carousel-dots');
        if (dotsWrap) dotsWrap.style.display = showNav ? 'flex' : 'none';

        currentIndex = 0;
        restartAutoplay();
    }

    function mountAll(rawList) {
        var normalized = rawList.map(normalizeSlide);
        sortedPromos = sortByPriceHighToLow(normalized);
        mountCarousel(sortedPromos.length ? sortedPromos : normalized);
        renderRankLists(sortedPromos);
    }

    async function loadPromoCarousel() {
        try {
            var data = await apiGet(API_ENDPOINTS.PROMOTIONS);
            if (Array.isArray(data) && data.length) {
                mountAll(data);
                return;
            }
        } catch (e) {
            console.warn('Không tải khuyến mãi:', e);
        }
        mountAll(FALLBACK_SLIDES);
    }

    function wireControls() {
        var prev = document.getElementById('promo-carousel-prev');
        var next = document.getElementById('promo-carousel-next');
        if (prev) {
            prev.addEventListener('click', function () {
                prevSlide();
                restartAutoplay();
            });
        }
        if (next) {
            next.addEventListener('click', function () {
                nextSlide();
                restartAutoplay();
            });
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        wireControls();
        loadPromoCarousel();
    });
})();
