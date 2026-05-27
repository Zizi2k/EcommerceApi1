(function () {
    var editingPromoId = null;
    var promoCache = [];

    function escHtml(s) {
        return String(s == null ? '' : s)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/"/g, '&quot;');
    }

    function setPromoMessages(success, error) {
        var s = document.getElementById('promo-success-message');
        var e = document.getElementById('promo-error-message');
        if (!s || !e) return;
        s.style.display = success ? 'block' : 'none';
        e.style.display = error ? 'block' : 'none';
        if (success) document.getElementById('promo-success-text').textContent = success;
        if (error) document.getElementById('promo-error-text').textContent = error;
    }

    function updatePromoModeUI() {
        var badge = document.getElementById('promo-mode-badge');
        var cancel = document.getElementById('promo-btn-cancel');
        var label = document.getElementById('promo-submit-label');
        if (!badge) return;
        if (editingPromoId != null) {
            badge.textContent = 'Đang sửa #' + editingPromoId;
            badge.classList.add('edit');
            if (cancel) cancel.style.display = 'block';
            if (label) label.textContent = 'Cập nhật khuyến mãi';
        } else {
            badge.textContent = 'Thêm mới';
            badge.classList.remove('edit');
            if (cancel) cancel.style.display = 'none';
            if (label) label.textContent = 'Lưu khuyến mãi';
        }
    }

    async function fillPromoProductSelect() {
        var sel = document.getElementById('promo-productId');
        if (!sel) return;
        try {
            var data = await apiGet(API_ENDPOINTS.PRODUCTS + '?page=1&pageSize=500');
            var list = (data && data.products) ? data.products : (Array.isArray(data) ? data : []);
            if (!list.length && typeof adminProductsCache !== 'undefined' && adminProductsCache.length) {
                list = adminProductsCache;
            }
            sel.innerHTML = '<option value="">— Chọn sản phẩm —</option>' +
                list.map(function (p) {
                    return '<option value="' + p.id + '">' + escHtml(p.name) + ' (' +
                        Number(p.price).toLocaleString('vi-VN') + ' ₫)</option>';
                }).join('');
        } catch (err) {
            console.warn(err);
            sel.innerHTML = '<option value="">Không tải được sản phẩm</option>';
        }
    }

    function resetPromoForm() {
        editingPromoId = null;
        document.getElementById('promo-id').value = '';
        document.getElementById('promo-productId').value = '';
        document.getElementById('promo-headline').value = '';
        document.getElementById('promo-subtitle').value = '';
        document.getElementById('promo-badgeText').value = 'HOT';
        document.getElementById('promo-promoPrice').value = '';
        document.getElementById('promo-sortOrder').value = '0';
        document.getElementById('promo-isActive').checked = true;
        setPromoMessages('', '');
        updatePromoModeUI();
    }

    function fillPromoForm(p) {
        editingPromoId = p.id;
        document.getElementById('promo-id').value = p.id;
        document.getElementById('promo-productId').value = String(p.productId);
        document.getElementById('promo-headline').value = p.headline || '';
        document.getElementById('promo-subtitle').value = p.subtitle || '';
        document.getElementById('promo-badgeText').value = p.badgeText || 'HOT';
        document.getElementById('promo-promoPrice').value = p.promoPrice != null ? p.promoPrice : '';
        document.getElementById('promo-sortOrder').value = p.sortOrder != null ? p.sortOrder : 0;
        document.getElementById('promo-isActive').checked = p.isActive !== false;
        setPromoMessages('', '');
        updatePromoModeUI();
        document.getElementById('promo-form-panel').scrollIntoView({ behavior: 'smooth', block: 'start' });
    }

    async function loadPromoTable() {
        var tbody = document.getElementById('promo-table-body');
        var loading = document.getElementById('promo-table-loading');
        var empty = document.getElementById('promo-table-empty');
        if (!tbody) return;

        loading.style.display = 'block';
        empty.style.display = 'none';
        var emptyMsgReset = document.getElementById('promo-table-empty-msg');
        if (emptyMsgReset) emptyMsgReset.textContent = 'Chưa có slide khuyến mãi. Thêm slide mới ở form bên phải.';
        tbody.innerHTML = '';

        try {
            var list = await apiGet(API_ENDPOINTS.PROMOTIONS_ALL);
            if (!Array.isArray(list)) list = [];
            promoCache = list;
            loading.style.display = 'none';

            var countEl = document.getElementById('promo-count-text');
            if (countEl) countEl.textContent = list.length + ' slide';

            if (!list.length) {
                if (countEl) countEl.textContent = '0 slide';
                empty.style.display = 'block';
                return;
            }
            empty.style.display = 'none';

            tbody.innerHTML = list.map(function (p) {
                var img = escHtml(p.imageUrl || '');
                var name = escHtml(p.productName || p.headline || '—');
                var status = p.isActive
                    ? '<span class="promo-status promo-status--on">Đang hiện</span>'
                    : '<span class="promo-status promo-status--off">Ẩn</span>';
                var badge = p.badgeText
                    ? '<span class="promo-badge-pill">' + escHtml(p.badgeText) + '</span>'
                    : '—';
                var price = p.promoPrice != null
                    ? Number(p.promoPrice).toLocaleString('vi-VN') + ' ₫'
                    : '—';
                return '<tr>' +
                    '<td><img class="thumb" src="' + (img || 'https://via.placeholder.com/48') + '" alt="" onerror="this.src=\'https://via.placeholder.com/48\'"></td>' +
                    '<td class="cell-name"><strong>' + name + '</strong><small>' + escHtml((p.subtitle || '').slice(0, 50)) + '</small></td>' +
                    '<td>' + badge + '</td>' +
                    '<td><strong>' + price + '</strong></td>' +
                    '<td><span class="promo-sort-num">' + (p.sortOrder != null ? p.sortOrder : 0) + '</span></td>' +
                    '<td>' + status + '</td>' +
                    '<td class="actions">' +
                    '<button type="button" class="btn-icon btn-edit" title="Sửa" onclick="editPromoById(' + p.id + ')"><i class="fa-solid fa-pen"></i></button>' +
                    '<button type="button" class="btn-icon btn-del" title="Xóa" onclick="deletePromo(' + p.id + ')"><i class="fa-solid fa-trash"></i></button>' +
                    '</td></tr>';
            }).join('');
        } catch (err) {
            loading.style.display = 'none';
            empty.style.display = 'block';
            var countErr = document.getElementById('promo-count-text');
            if (countErr) countErr.textContent = '—';
            var emptyMsg = document.getElementById('promo-table-empty-msg');
            if (emptyMsg) emptyMsg.textContent = 'Không tải được: ' + (err.message || err);
        }
    }

    window.editPromoById = function (id) {
        var p = promoCache.find(function (x) { return x.id === id; });
        if (p) fillPromoForm(p);
    };

    window.deletePromo = async function (id) {
        if (!confirm('Xóa slide khuyến mãi này?')) return;
        try {
            await apiDelete(API_ENDPOINTS.PROMOTIONS + '/' + id);
            setPromoMessages('Đã xóa khuyến mãi.', '');
            if (editingPromoId === id) resetPromoForm();
            loadPromoTable();
        } catch (e) {
            setPromoMessages('', 'Xóa thất bại: ' + (e.message || e));
        }
    };

    window.submitPromoForm = async function () {
        if (!checkAuth()) return;
        var productId = parseInt(document.getElementById('promo-productId').value, 10);
        if (!Number.isFinite(productId) || productId < 1) {
            setPromoMessages('', 'Vui lòng chọn sản phẩm.');
            return;
        }

        var payload = {
            productId: productId,
            headline: document.getElementById('promo-headline').value.trim() || null,
            subtitle: document.getElementById('promo-subtitle').value.trim() || null,
            badgeText: document.getElementById('promo-badgeText').value.trim() || 'HOT',
            promoPrice: document.getElementById('promo-promoPrice').value
                ? parseFloat(document.getElementById('promo-promoPrice').value) : null,
            sortOrder: parseInt(document.getElementById('promo-sortOrder').value, 10) || 0,
            isActive: document.getElementById('promo-isActive').checked
        };

        try {
            if (editingPromoId != null) {
                await apiPut(API_ENDPOINTS.PROMOTIONS + '/' + editingPromoId, payload);
                setPromoMessages('Đã cập nhật khuyến mãi.', '');
                resetPromoForm();
            } else {
                await apiPost(API_ENDPOINTS.PROMOTIONS, payload);
                setPromoMessages('Đã thêm sản phẩm khuyến mãi lên carousel.', '');
                resetPromoForm();
            }
            loadPromoTable();
        } catch (e) {
            setPromoMessages('', e.message || 'Lưu thất bại.');
        }
    };

    window.resetPromoForm = resetPromoForm;
    window.loadPromoTable = loadPromoTable;
    window.fillPromoProductSelect = fillPromoProductSelect;

    document.addEventListener('DOMContentLoaded', function () {
        if (!document.getElementById('promo-form-panel')) return;
        fillPromoProductSelect();
        loadPromoTable();
        updatePromoModeUI();
    });
})();
