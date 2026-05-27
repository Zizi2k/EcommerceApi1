(function () {
    function esc(s) {
        return String(s == null ? '' : s)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/"/g, '&quot;');
    }

    function fmtDate(iso) {
        if (!iso) return '';
        try {
            return new Date(iso).toLocaleString('vi-VN', { dateStyle: 'short', timeStyle: 'short' });
        } catch (e) {
            return iso;
        }
    }

    window.formatProductRatingHtml = function (avg, count, opts) {
        var n = Number(count) || 0;
        if (n <= 0) return '';
        var a = Number(avg) || 0;
        var rounded = Math.round(a);
        var stars = '';
        for (var i = 1; i <= 5; i++) {
            stars += '<i class="fa-solid fa-star product-star' + (i <= rounded ? ' is-on' : '') + '"></i>';
        }
        var compact = opts && opts.compact;
        return '<div class="product-rating' + (compact ? ' product-rating--compact' : '') + '">' +
            '<span class="product-rating-stars" aria-label="' + a.toFixed(1) + ' trên 5">' + stars + '</span>' +
            '<span class="product-rating-avg">' + a.toFixed(1) + '</span>' +
            '<span class="product-rating-count">(' + n + ' đánh giá)</span>' +
            '</div>';
    };

    window.renderProductReviewsListHtml = function (reviews) {
        if (!reviews || !reviews.length) {
            return '<p class="product-reviews-empty">Chưa có đánh giá nào.</p>';
        }
        return reviews.map(function (r) {
            var stars = '';
            for (var i = 1; i <= 5; i++) {
                stars += '<i class="fa-solid fa-star product-star' + (i <= r.rating ? ' is-on' : '') + '"></i>';
            }
            return '<article class="product-review-item">' +
                '<div class="product-review-item-head">' +
                '<strong>' + esc(r.reviewerName || 'Khách') + '</strong>' +
                '<span class="product-review-item-stars">' + stars + '</span>' +
                '</div>' +
                (r.note ? '<p class="product-review-item-note">' + esc(r.note) + '</p>' : '') +
                '<time class="product-review-item-time">' + fmtDate(r.createdAtUtc) + '</time>' +
                '</article>';
        }).join('');
    };
})();
