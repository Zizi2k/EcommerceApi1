(function () {
    var currentProduct = null;
    var toastTimer = null;

    function escapeHtml(str) {
        return String(str == null ? '' : str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/"/g, '&quot;');
    }

    function getProductIdFromUrl() {
        var params = new URLSearchParams(window.location.search);
        var id = parseInt(params.get('id'), 10);
        return Number.isFinite(id) && id > 0 ? id : null;
    }

    function getSavedProducts() {
        try {
            var saved = JSON.parse(localStorage.getItem('savedProducts') || '[]');
            return Array.isArray(saved) ? saved : [];
        } catch (e) {
            return [];
        }
    }

    function categoryLabel(p) {
        return typeof resolveCategoryName === 'function'
            ? resolveCategoryName(p.categoryId, p.categoryName)
            : (p.categoryName || '');
    }

    function showToast(message) {
        var el = document.getElementById('product-toast');
        if (!el) return;
        el.textContent = message;
        el.classList.add('is-visible');
        clearTimeout(toastTimer);
        toastTimer = setTimeout(function () {
            el.classList.remove('is-visible');
        }, 2800);
    }

    function setLoading() {
        document.getElementById('product-detail-root').innerHTML =
            '<div class="product-detail-loading">' +
            '<i class="fa-solid fa-spinner fa-spin"></i>' +
            '<p>Đang tải thông tin sản phẩm...</p>' +
            '</div>';
    }

    function showError(message) {
        document.getElementById('product-detail-root').innerHTML =
            '<div class="product-detail-error">' +
            '<i class="fa-solid fa-box-open"></i>' +
            '<p>' + escapeHtml(message || 'Không tìm thấy sản phẩm.') + '</p>' +
            '<a href="index.html" class="btn-product-secondary"><i class="fa-solid fa-home"></i> Về trang chủ</a>' +
            '</div>';
        document.title = 'Không tìm thấy - Group 1 Store';
    }

    function renderProduct(p) {
        currentProduct = p;
        var cat = categoryLabel(p);
        var stock = Number(p.stock) || 0;
        var inStock = stock > 0;
        var maxQty = Math.max(1, Math.min(stock || 99, 99));

        document.title = (p.name || 'Sản phẩm') + ' - Group 1 Store';
        document.getElementById('breadcrumb-name').textContent = p.name;

        var stockHtml = inStock
            ? '<span class="product-meta-item"><i class="fa-solid fa-circle-check"></i> Còn ' + stock + ' sản phẩm</span>'
            : '<span class="product-meta-item" style="color:#b91c1c;border-color:#fecaca;background:#fef2f2;"><i class="fa-solid fa-circle-xmark"></i> Hết hàng</span>';

        document.getElementById('product-detail-root').innerHTML =
            '<article class="product-detail-card">' +
            '<div class="product-detail-gallery">' +
            '<span class="product-detail-badge">HOT</span>' +
            '<img id="product-image" src="' + escapeHtml(p.imageUrl || 'https://via.placeholder.com/400') + '" alt="' + escapeHtml(p.name) + '" onerror="this.src=\'https://via.placeholder.com/400x400?text=No+Image\'">' +
            '</div>' +
            '<div class="product-detail-body">' +
            (cat ? '<p class="product-detail-category">' + escapeHtml(cat) + '</p>' : '') +
            '<h1 class="product-detail-title">' + escapeHtml(p.name) + '</h1>' +
            '<p class="product-detail-desc">' + escapeHtml(p.description || 'Sản phẩm công nghệ chất lượng cao từ Group 1 Store.') + '</p>' +
            '<div class="product-detail-meta">' + stockHtml +
            '<span class="product-meta-item"><i class="fa-solid fa-truck-fast"></i> Giao hàng toàn quốc</span>' +
            '<span class="product-meta-item"><i class="fa-solid fa-shield-halved"></i> Bảo hành chính hãng</span>' +
            '</div>' +
            '<div class="product-detail-price-row">' +
            '<span class="product-detail-price">' + Number(p.price).toLocaleString('vi-VN') + ' VNĐ</span>' +
            '</div>' +
            '<div class="product-detail-actions">' +
            '<div class="qty-control" role="group" aria-label="Số lượng">' +
            '<button type="button" id="qty-minus" aria-label="Giảm"' + (inStock ? '' : ' disabled') + '><i class="fa-solid fa-minus"></i></button>' +
            '<input type="number" id="qty-input" value="1" min="1" max="' + maxQty + '"' + (inStock ? '' : ' disabled') + '>' +
            '<button type="button" id="qty-plus" aria-label="Tăng"' + (inStock ? '' : ' disabled') + '><i class="fa-solid fa-plus"></i></button>' +
            '</div>' +
            '<button type="button" class="btn-product-primary" id="btn-add-cart"' + (inStock ? '' : ' disabled') + '>' +
            '<i class="fa-solid fa-cart-plus"></i> Thêm vào giỏ hàng' +
            '</button>' +
            '<a href="index.html" class="btn-product-secondary"><i class="fa-solid fa-arrow-left"></i> Về trang chủ</a>' +
            '</div>' +
            '</div>' +
            '</article>';

        var qtyInput = document.getElementById('qty-input');
        var btnMinus = document.getElementById('qty-minus');
        var btnPlus = document.getElementById('qty-plus');
        var btnAdd = document.getElementById('btn-add-cart');

        function getQty() {
            var n = parseInt(qtyInput.value, 10);
            if (!Number.isFinite(n) || n < 1) return 1;
            return Math.min(n, maxQty);
        }

        function setQty(n) {
            qtyInput.value = String(Math.max(1, Math.min(n, maxQty)));
            btnMinus.disabled = getQty() <= 1;
            btnPlus.disabled = getQty() >= maxQty;
        }

        btnMinus.addEventListener('click', function () { setQty(getQty() - 1); });
        btnPlus.addEventListener('click', function () { setQty(getQty() + 1); });
        qtyInput.addEventListener('change', function () { setQty(getQty()); });

        btnAdd.addEventListener('click', function () {
            addToCart(p.id, getQty());
        });

        setQty(1);
    }

    async function loadProduct() {
        var id = getProductIdFromUrl();
        if (!id) {
            showError('Liên kết không hợp lệ.');
            return;
        }

        setLoading();

        try {
            var p = await apiGet(API_ENDPOINTS.PRODUCTS + '/' + id);
            if (!p || !p.id) throw new Error('empty');
            renderProduct(p);
            return;
        } catch (e) {
            console.warn('API product detail:', e);
        }

        var local = getSavedProducts().find(function (x) { return x.id === id; });
        if (local) {
            renderProduct(local);
            return;
        }

        showError('Sản phẩm không tồn tại hoặc đã bị xóa.');
    }

    async function addToCart(productId, quantity) {
        if (!checkAuth()) return;
        var btn = document.getElementById('btn-add-cart');
        if (btn) {
            btn.disabled = true;
            btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Đang thêm...';
        }
        try {
            await apiPost(API_ENDPOINTS.CART_ADD, {
                productId: productId,
                quantity: quantity || 1
            });
            showToast('Đã thêm ' + (quantity || 1) + ' sản phẩm vào giỏ hàng!');
            await updateCartCount(quantity || 1);
        } catch (error) {
            alert('Không thể thêm vào giỏ! ' + (error.message || ''));
        } finally {
            if (btn && currentProduct) {
                var stock = Number(currentProduct.stock) || 0;
                btn.disabled = stock <= 0;
                btn.innerHTML = '<i class="fa-solid fa-cart-plus"></i> Thêm vào giỏ hàng';
            }
        }
    }

    window.addEventListener('DOMContentLoaded', function () {
        loadProduct();
    });
})();
