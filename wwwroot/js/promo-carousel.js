(function () {
    var slides = [];
    var currentIndex = 0;
    var autoplayTimer = null;
    var AUTOPLAY_MS = 5500;

    var FALLBACK_SLIDES = [
        {
            headline: 'Công Nghệ Hiện Đại',
            subtitle: 'Khám phá những sản phẩm công nghệ mới nhất với giá tốt nhất',
            badgeText: 'MỚI',
            displayPrice: null,
            price: null,
            productId: null,
            imageUrl: 'https://images.unsplash.com/photo-1519389950473-47ba0277781c?q=80&w=1920'
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

    function normalizeSlide(raw) {
        var headline = (raw.headline && raw.headline.trim()) || raw.productName || 'Sản phẩm khuyến mãi';
        var subtitle = (raw.subtitle && raw.subtitle.trim()) || raw.description || '';
        var badge = (raw.badgeText && raw.badgeText.trim()) || 'KHUYẾN MÃI';
        var displayPrice = raw.displayPrice != null ? raw.displayPrice : (raw.promoPrice != null ? raw.promoPrice : raw.price);
        var original = raw.price;
        var showOld = displayPrice != null && original != null && Number(displayPrice) < Number(original);

        return {
            id: raw.id,
            productId: raw.productId,
            headline: headline,
            subtitle: subtitle.slice(0, 160) + (subtitle.length > 160 ? '…' : ''),
            badgeText: badge,
            displayPrice: displayPrice,
            price: original,
            showOldPrice: showOld,
            imageUrl: raw.imageUrl || 'https://images.unsplash.com/photo-1519389950473-47ba0277781c?q=80&w=1920'
        };
    }

    function renderSlideHtml(s, index) {
        var priceHtml = '';
        if (s.displayPrice != null) {
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

        var bg = escapeHtml(s.imageUrl);
        var active = index === 0 ? ' is-active' : '';

        return '<article class="promo-slide' + active + '" data-index="' + index + '" style="background-image:url(\'' + bg + '\')">' +
            '<div class="promo-slide-content">' +
            '<span class="promo-slide-badge">' + escapeHtml(s.badgeText) + '</span>' +
            '<h1>' + escapeHtml(s.headline) + '</h1>' +
            '<p>' + escapeHtml(s.subtitle) + '</p>' +
            priceHtml +
            '<div class="promo-slide-actions">' + detailBtn + shopBtn + '</div>' +
            '</div></article>';
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
        var nodes = document.querySelectorAll('.promo-slide');
        nodes.forEach(function (el, i) {
            el.classList.toggle('is-active', i === currentIndex);
        });
        document.querySelectorAll('.promo-dot').forEach(function (dot, i) {
            dot.classList.toggle('is-active', i === currentIndex);
        });
    }

    function nextSlide() {
        goToSlide(currentIndex + 1);
    }

    function prevSlide() {
        goToSlide(currentIndex - 1);
    }

    function restartAutoplay() {
        clearInterval(autoplayTimer);
        if (slides.length > 1) {
            autoplayTimer = setInterval(nextSlide, AUTOPLAY_MS);
        }
    }

    function mountCarousel(list) {
        slides = list.map(normalizeSlide);
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

    async function loadPromoCarousel() {
        var inner = document.getElementById('promo-carousel-inner');
        if (!inner) return;

        try {
            var data = await apiGet(API_ENDPOINTS.PROMOTIONS);
            if (Array.isArray(data) && data.length) {
                mountCarousel(data);
                return;
            }
        } catch (e) {
            console.warn('Không tải carousel khuyến mãi:', e);
        }

        mountCarousel(FALLBACK_SLIDES);
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
