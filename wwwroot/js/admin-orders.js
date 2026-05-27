(function () {
    var ordersCache = [];
    var selectedOrderId = null;
    var selectedRating = 0;
    var orderPollTimer = null;
    var monthlyReportCache = null;

    var STATUS_OPTIONS = [
        { value: 'Preparing', label: 'Đang chuẩn bị' },
        { value: 'Delivering', label: 'Đang giao' },
        { value: 'Delivered', label: 'Đã giao' }
    ];

    function escHtml(s) {
        return String(s == null ? '' : s)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/"/g, '&quot;');
    }

    function statusBadgeClass(status) {
        var s = (status || '').toLowerCase();
        if (s === 'delivering') return 'order-status-badge--delivering';
        if (s === 'delivered') return 'order-status-badge--delivered';
        if (s === 'cancelled') return 'order-status-badge--cancelled';
        return 'order-status-badge--preparing';
    }

    function showOrderMsg(text, isError) {
        var el = document.getElementById('order-admin-msg');
        if (!el) return;
        if (!text) {
            el.style.display = 'none';
            el.className = 'order-msg';
            el.textContent = '';
            return;
        }
        el.style.display = 'block';
        el.className = 'order-msg ' + (isError ? 'err' : 'ok');
        el.textContent = text;
    }

    function formatVnd(n) {
        return Number(n || 0).toLocaleString('vi-VN') + ' ₫';
    }

    function formatDate(iso) {
        if (!iso) return '—';
        try {
            return new Date(iso).toLocaleString('vi-VN', { dateStyle: 'short', timeStyle: 'short' });
        } catch (e) {
            return iso;
        }
    }

    function getReportMonthParts() {
        var monthEl = document.getElementById('order-report-month');
        var val = monthEl && monthEl.value ? monthEl.value : '';
        if (!val || val.indexOf('-') < 0) {
            var now = new Date();
            return { year: now.getFullYear(), month: now.getMonth() + 1 };
        }
        var parts = val.split('-');
        return {
            year: parseInt(parts[0], 10),
            month: parseInt(parts[1], 10)
        };
    }

    function showReportMsg(text, isError) {
        var el = document.getElementById('order-report-msg');
        if (!el) return;
        if (!text) {
            el.style.display = 'none';
            el.className = 'order-msg';
            el.textContent = '';
            return;
        }
        el.style.display = 'block';
        el.className = 'order-msg ' + (isError ? 'err' : 'ok');
        el.textContent = text;
    }

    function renderMonthlyReport(report) {
        monthlyReportCache = report || null;
        var rows = report && Array.isArray(report.products) ? report.products : [];
        var body = document.getElementById('order-report-body');
        var empty = document.getElementById('order-report-empty');
        if (!body) return;

        document.getElementById('report-delivered-orders').textContent = String(report && report.deliveredOrderCount || 0);
        document.getElementById('report-total-quantity').textContent = String(report && report.totalQuantity || 0);
        document.getElementById('report-total-revenue').textContent = formatVnd(report && report.totalRevenue || 0);
        document.getElementById('report-total-capital').textContent = formatVnd(report && report.totalCapital || 0);
        document.getElementById('report-total-profit').textContent = formatVnd(report && report.totalProfit || 0);

        body.innerHTML = rows.map(function (r) {
            return '<tr>' +
                '<td>#' + r.productId + '</td>' +
                '<td>' + escHtml(r.productName) + '</td>' +
                '<td>' + r.quantity + '</td>' +
                '<td>' + formatVnd(r.revenue) + '</td>' +
                '<td>' + formatVnd(r.capital) + '</td>' +
                '<td><strong>' + formatVnd(r.profit) + '</strong></td>' +
                '</tr>';
        }).join('');

        if (!rows.length) {
            if (empty) empty.style.display = 'block';
        } else if (empty) {
            empty.style.display = 'none';
        }
    }

    function getFilteredOrders() {
        var status = document.getElementById('order-filter-status');
        var search = document.getElementById('order-filter-search');
        var statusVal = status ? status.value : '';
        var q = search ? search.value.trim().toLowerCase() : '';

        return ordersCache.filter(function (o) {
            if (statusVal && o.status !== statusVal) return false;
            if (!q) return true;
            return String(o.id).indexOf(q) >= 0 ||
                (o.customerName || '').toLowerCase().indexOf(q) >= 0 ||
                (o.customerPhone || '').toLowerCase().indexOf(q) >= 0 ||
                (o.accountUsername || '').toLowerCase().indexOf(q) >= 0;
        });
    }

    function renderOrderPicker() {
        var list = getFilteredOrders();
        var sel = document.getElementById('order-picker-select');
        var countEl = document.getElementById('order-count-text');
        if (!sel) return;

        var prev = sel.value;
        sel.innerHTML = '<option value="">— Chọn đơn hàng —</option>' +
            list.map(function (o) {
                var mark = o.hasCancelRequest ? '[YC hủy] ' : '';
                return '<option value="' + o.id + '">#' + o.id + ' · ' +
                    mark + escHtml(o.customerName || o.accountUsername || 'Khách') + ' · ' +
                    formatVnd(o.totalAmount) + '</option>';
            }).join('');

        if (countEl) {
            countEl.textContent = ordersCache.length
                ? (list.length + ' / ' + ordersCache.length + ' đơn')
                : '0 đơn';
        }

        if (prev && list.some(function (o) { return String(o.id) === prev; })) {
            sel.value = prev;
        } else {
            sel.value = '';
        }
        onOrderPickerChange();
    }

    function renderOrderDetail(order) {
        var detail = document.getElementById('order-detail-panel');
        var empty = document.getElementById('order-detail-empty');
        if (!detail || !empty) return;

        if (!order) {
            detail.style.display = 'none';
            empty.style.display = 'block';
            return;
        }

        empty.style.display = 'none';
        detail.style.display = 'block';

        document.getElementById('order-detail-title').textContent = 'Đơn #' + order.id;
        var badge = document.getElementById('order-detail-status-badge');
        if (badge) {
            badge.className = 'order-status-badge ' + statusBadgeClass(order.status);
            badge.textContent = order.statusLabel || order.status;
        }

        document.getElementById('order-detail-customer').innerHTML =
            '<strong>' + escHtml(order.customerName || order.accountUsername || '—') + '</strong>' +
            (order.customerPhone ? '<br>SĐT: ' + escHtml(order.customerPhone) : '') +
            (order.shippingAddress ? '<br>Địa chỉ: ' + escHtml(order.shippingAddress) : '') +
            '<br>Thanh toán: ' + escHtml(order.paymentMethod) +
            '<br>Ngày đặt: ' + formatDate(order.createdAtUtc) +
            '<br><strong>Tổng: ' + formatVnd(order.totalAmount) + '</strong>';

        var itemsEl = document.getElementById('order-detail-items');
        if (itemsEl) {
            itemsEl.innerHTML = (order.items || []).map(function (it) {
                return '<li><span>' + escHtml(it.productName) + ' × ' + it.quantity + '</span>' +
                    '<span>' + formatVnd(it.lineTotal) + '</span></li>';
            }).join('') || '<li>—</li>';
        }

        var cancelAlert = document.getElementById('order-cancel-alert');
        if (cancelAlert) {
            var showCancelInfo = order.hasCancelRequest ||
                (order.cancelReason && order.status === 'Cancelled');
            if (showCancelInfo) {
                cancelAlert.style.display = 'block';
                var title = order.status === 'Cancelled'
                    ? '<strong>Đơn đã hủy.</strong> Lý do: '
                    : '<strong>Yêu cầu hủy đơn từ khách:</strong> ';
                cancelAlert.innerHTML = title + escHtml(order.cancelReason || '—') +
                    (order.cancelNote ? '<br>Ghi chú: ' + escHtml(order.cancelNote) : '') +
                    (order.cancelRequestedAtUtc ? '<br>Thời gian gửi: ' + formatDate(order.cancelRequestedAtUtc) : '');
            } else {
                cancelAlert.style.display = 'none';
                cancelAlert.innerHTML = '';
            }
        }

        var cancelActions = document.getElementById('order-cancel-actions');
        if (cancelActions) {
            var canRespond = order.canRespondToCancelRequest ||
                (order.hasCancelRequest && order.status !== 'Delivered' && order.status !== 'Cancelled');
            cancelActions.style.display = canRespond ? 'flex' : 'none';
        }

        var statusForm = document.querySelector('.order-status-form');
        if (statusForm) {
            statusForm.style.display = order.status === 'Cancelled' ? 'none' : 'block';
        }

        var statusSel = document.getElementById('order-status-select');
        if (statusSel) statusSel.value = order.status;

        var customerReviewBlock = document.getElementById('order-customer-review-block');
        if (customerReviewBlock) {
            if (order.status === 'Delivered' || order.customerRating) {
                customerReviewBlock.style.display = 'block';
                if (order.customerRating) {
                    var who = escHtml(order.customerName || order.accountUsername || 'Khách');
                    var starsHtml = '';
                    for (var si = 1; si <= 5; si++) {
                        starsHtml += '<i class="fa-solid fa-star product-star' +
                            (si <= order.customerRating ? ' is-on' : '') + '"></i>';
                    }
                    starsHtml = '<span class="product-rating-stars">' + starsHtml + '</span> <strong>' +
                        order.customerRating + '/5</strong>';
                    customerReviewBlock.innerHTML =
                        '<h4><i class="fa-solid fa-user-check"></i> Đánh giá từ khách hàng</h4>' +
                        '<div><strong>' + who + '</strong></div>' +
                        '<div style="margin-top:6px;">' + starsHtml + '</div>' +
                        (order.customerReviewNote
                            ? '<p style="margin:8px 0 0;font-size:13px;color:#334155;">' + escHtml(order.customerReviewNote) + '</p>'
                            : '') +
                        '<p class="order-customer-review-meta">' +
                        (order.customerReviewedAtUtc ? 'Gửi lúc: ' + formatDate(order.customerReviewedAtUtc) : '') +
                        '</p>';
                } else {
                    customerReviewBlock.innerHTML =
                        '<h4><i class="fa-solid fa-user-clock"></i> Đánh giá từ khách hàng</h4>' +
                        '<p class="order-customer-review-wait">Chưa có đánh giá. Trang sẽ tự cập nhật khi khách gửi.</p>';
                }
            } else {
                customerReviewBlock.style.display = 'none';
                customerReviewBlock.innerHTML = '';
            }
        }

        var reviewBlock = document.getElementById('order-review-block');
        var canReview = order.status === 'Delivered';
        if (reviewBlock) {
            reviewBlock.classList.toggle('is-locked', !canReview);
        }
        var reviewHint = document.getElementById('order-review-hint');
        if (reviewHint) {
            reviewHint.textContent = canReview
                ? 'Đánh giá chất lượng xử lý / giao hàng cho đơn đã giao.'
                : 'Chỉ đánh giá khi đơn ở trạng thái Đã giao.';
        }

        selectedRating = order.adminRating || 0;
        renderStars(selectedRating);

        var note = document.getElementById('order-review-note');
        if (note) note.value = order.adminReviewNote || '';

        var existing = document.getElementById('order-review-existing');
        if (existing) {
            if (order.adminRating) {
                existing.style.display = 'block';
                existing.innerHTML = '<strong>Đã đánh giá:</strong> ' + order.adminRating + '/5 sao' +
                    (order.adminReviewNote ? ' — ' + escHtml(order.adminReviewNote) : '') +
                    (order.adminReviewedAtUtc ? '<br><small>' + formatDate(order.adminReviewedAtUtc) + '</small>' : '');
            } else {
                existing.style.display = 'none';
                existing.innerHTML = '';
            }
        }
    }

    function renderStars(rating) {
        var row = document.getElementById('order-star-row');
        if (!row) return;
        row.innerHTML = '';
        for (var i = 1; i <= 5; i++) {
            (function (star) {
                var btn = document.createElement('button');
                btn.type = 'button';
                btn.className = 'order-star-btn' + (star <= rating ? ' is-on' : '');
                btn.innerHTML = '<i class="fa-solid fa-star"></i>';
                btn.title = star + ' sao';
                btn.addEventListener('click', function () {
                    selectedRating = star;
                    renderStars(selectedRating);
                });
                row.appendChild(btn);
            })(i);
        }
    }

    function stopOrderDetailPolling() {
        if (orderPollTimer) {
            clearInterval(orderPollTimer);
            orderPollTimer = null;
        }
    }

    function startOrderDetailPolling() {
        stopOrderDetailPolling();
        if (!selectedOrderId) return;
        orderPollTimer = setInterval(refreshSelectedOrderFromApi, 10000);
    }

    async function refreshSelectedOrderFromApi() {
        if (!selectedOrderId || !getToken()) return;
        try {
            var o = await apiGet(API_ENDPOINTS.ADMIN_ORDERS + '/' + selectedOrderId);
            if (!o || !o.id) return;
            var idx = ordersCache.findIndex(function (x) { return x.id === o.id; });
            if (idx >= 0) ordersCache[idx] = o;
            else ordersCache.push(o);
            if (selectedOrderId === o.id) renderOrderDetail(o);
        } catch (e) { /* ignore */ }
    }

    function onOrderPickerChange() {
        var sel = document.getElementById('order-picker-select');
        var id = sel && parseInt(sel.value, 10);
        selectedOrderId = id && !isNaN(id) ? id : null;
        var order = selectedOrderId
            ? ordersCache.find(function (o) { return o.id === selectedOrderId; })
            : null;
        renderOrderDetail(order);
        showOrderMsg('', false);
        if (selectedOrderId) {
            startOrderDetailPolling();
        } else {
            stopOrderDetailPolling();
        }
    }

    async function loadAdminOrders() {
        var loading = document.getElementById('orders-loading');
        var tools = document.getElementById('order-picker-tools');
        var empty = document.getElementById('orders-list-empty');
        if (loading) loading.style.display = 'block';
        if (tools) tools.style.display = 'none';
        if (empty) empty.style.display = 'none';
        showOrderMsg('', false);

        try {
            var list = await apiGet(API_ENDPOINTS.ADMIN_ORDERS);
            if (!Array.isArray(list)) list = [];
            ordersCache = list;
            if (loading) loading.style.display = 'none';

            if (!list.length) {
                if (empty) empty.style.display = 'block';
                renderOrderPicker();
                renderOrderDetail(null);
                return;
            }
            if (tools) tools.style.display = 'block';
            renderOrderPicker();
        } catch (err) {
            if (loading) loading.style.display = 'none';
            if (empty) {
                empty.style.display = 'block';
                var msg = document.getElementById('orders-list-empty-msg');
                if (msg) msg.textContent = 'Không tải được: ' + (err.message || err);
            }
            ordersCache = [];
            renderOrderPicker();
        }
    }

    window.loadMonthlyOrderReport = async function () {
        if (!checkAuth()) return;
        var monthPart = getReportMonthParts();
        showReportMsg('', false);
        try {
            var url = API_ENDPOINTS.ADMIN_ORDERS_REPORT_MONTHLY +
                '?year=' + encodeURIComponent(monthPart.year) +
                '&month=' + encodeURIComponent(monthPart.month);
            var report = await apiGet(url);
            renderMonthlyReport(report);
            showReportMsg('Đã tải báo cáo ' + (report.periodLabel || ''), false);
        } catch (e) {
            renderMonthlyReport(null);
            showReportMsg(e.message || 'Không tải được báo cáo.', true);
        }
    };

    window.exportMonthlyOrderReportExcel = async function () {
        if (!checkAuth()) return;
        var monthPart = getReportMonthParts();
        var token = getToken();
        if (!token) return;

        try {
            var exportUrl = API_ENDPOINTS.ADMIN_ORDERS_REPORT_MONTHLY + '/export' +
                '?year=' + encodeURIComponent(monthPart.year) +
                '&month=' + encodeURIComponent(monthPart.month);
            var res = await fetch(exportUrl, {
                headers: { 'Authorization': 'Bearer ' + token }
            });
            if (!res.ok) throw new Error('Xuất Excel thất bại.');
            var blob = await res.blob();
            var dlUrl = URL.createObjectURL(blob);
            var a = document.createElement('a');
            a.href = dlUrl;
            a.download = 'bao-cao-don-da-giao-' + monthPart.year + '-' +
                String(monthPart.month).padStart(2, '0') + '.xlsx';
            document.body.appendChild(a);
            a.click();
            a.remove();
            URL.revokeObjectURL(dlUrl);
            showReportMsg('Đã xuất file Excel.', false);
            if (window.AppNotify && typeof window.AppNotify.success === 'function') {
                window.AppNotify.success('Xuất Excel thành công.');
            }
        } catch (e) {
            showReportMsg(e.message || 'Không xuất được Excel.', true);
        }
    };

    window.saveOrderStatus = async function () {
        if (!checkAuth() || !selectedOrderId) {
            showOrderMsg('Chọn đơn hàng trước.', true);
            return;
        }
        var statusSel = document.getElementById('order-status-select');
        var status = statusSel ? statusSel.value : '';
        if (!status) {
            showOrderMsg('Chọn trạng thái.', true);
            return;
        }
        try {
            await apiPatch(API_ENDPOINTS.ADMIN_ORDERS + '/' + selectedOrderId + '/status', { status: status });
            showOrderMsg('Đã cập nhật trạng thái đơn #' + selectedOrderId + '.', false);
            if (typeof refreshNotifications === 'function') refreshNotifications();
            await loadAdminOrders();
            if (typeof loadCustomerRankings === 'function') {
                await loadCustomerRankings();
            }
            var sel = document.getElementById('order-picker-select');
            if (sel) sel.value = String(selectedOrderId);
            onOrderPickerChange();
        } catch (e) {
            showOrderMsg(e.message || 'Cập nhật thất bại.', true);
        }
    };

    window.acceptOrderCancel = async function () {
        if (!checkAuth() || !selectedOrderId) {
            showOrderMsg('Chọn đơn hàng trước.', true);
            return;
        }
        var order = ordersCache.find(function (o) { return o.id === selectedOrderId; });
        if (!order || (!order.canRespondToCancelRequest && !order.hasCancelRequest)) {
            showOrderMsg('Đơn này không có yêu cầu hủy để xử lý.', true);
            return;
        }
        if (!confirm('Chấp nhận hủy đơn #' + selectedOrderId + '?')) return;
        try {
            await apiPost(API_ENDPOINTS.ADMIN_ORDERS + '/' + selectedOrderId + '/cancel-request/accept', {});
            showOrderMsg('Đã chấp nhận hủy đơn #' + selectedOrderId + '.', false);
            if (typeof refreshNotifications === 'function') refreshNotifications();
            await loadAdminOrders();
            var sel = document.getElementById('order-picker-select');
            if (sel) sel.value = String(selectedOrderId);
            onOrderPickerChange();
        } catch (e) {
            showOrderMsg(e.message || 'Chấp nhận hủy thất bại.', true);
        }
    };

    window.rejectOrderCancel = async function () {
        if (!checkAuth() || !selectedOrderId) {
            showOrderMsg('Chọn đơn hàng trước.', true);
            return;
        }
        var order = ordersCache.find(function (o) { return o.id === selectedOrderId; });
        if (!order || (!order.canRespondToCancelRequest && !order.hasCancelRequest)) {
            showOrderMsg('Đơn này không có yêu cầu hủy để xử lý.', true);
            return;
        }
        if (!confirm('Từ chối yêu cầu hủy đơn #' + selectedOrderId + '?')) return;
        try {
            await apiPost(API_ENDPOINTS.ADMIN_ORDERS + '/' + selectedOrderId + '/cancel-request/reject', {});
            showOrderMsg('Đã từ chối yêu cầu hủy đơn #' + selectedOrderId + '.', false);
            if (typeof refreshNotifications === 'function') refreshNotifications();
            await loadAdminOrders();
            var sel = document.getElementById('order-picker-select');
            if (sel) sel.value = String(selectedOrderId);
            onOrderPickerChange();
        } catch (e) {
            showOrderMsg(e.message || 'Từ chối yêu cầu hủy thất bại.', true);
        }
    };

    window.saveOrderReview = async function () {
        if (!checkAuth() || !selectedOrderId) {
            showOrderMsg('Chọn đơn hàng trước.', true);
            return;
        }
        var order = ordersCache.find(function (o) { return o.id === selectedOrderId; });
        if (!order || order.status !== 'Delivered') {
            showOrderMsg('Chỉ đánh giá khi đơn ở trạng thái Đã giao.', true);
            return;
        }
        if (selectedRating < 1 || selectedRating > 5) {
            showOrderMsg('Chọn số sao từ 1 đến 5.', true);
            return;
        }
        var noteEl = document.getElementById('order-review-note');
        try {
            await apiPut(API_ENDPOINTS.ADMIN_ORDERS + '/' + selectedOrderId + '/review', {
                rating: selectedRating,
                note: noteEl ? noteEl.value.trim() : ''
            });
            showOrderMsg('Đã lưu đánh giá đơn #' + selectedOrderId + '.', false);
            await loadAdminOrders();
            var sel = document.getElementById('order-picker-select');
            if (sel) sel.value = String(selectedOrderId);
            onOrderPickerChange();
        } catch (e) {
            showOrderMsg(e.message || 'Lưu đánh giá thất bại.', true);
        }
    };

    function wireOrderAdminUi() {
        var search = document.getElementById('order-filter-search');
        var status = document.getElementById('order-filter-status');
        var picker = document.getElementById('order-picker-select');
        if (search && !search._wired) {
            search._wired = true;
            search.addEventListener('input', renderOrderPicker);
        }
        if (status && !status._wired) {
            status._wired = true;
            status.addEventListener('change', renderOrderPicker);
        }
        if (picker && !picker._wired) {
            picker._wired = true;
            picker.addEventListener('change', onOrderPickerChange);
        }
        var statusSel = document.getElementById('order-status-select');
        if (statusSel && !statusSel.innerHTML) {
            statusSel.innerHTML = STATUS_OPTIONS.map(function (o) {
                return '<option value="' + o.value + '">' + o.label + '</option>';
            }).join('');
        }
        var monthEl = document.getElementById('order-report-month');
        if (monthEl && !monthEl.value) {
            var now = new Date();
            monthEl.value = now.getFullYear() + '-' + String(now.getMonth() + 1).padStart(2, '0');
        }
    }

    window.loadAdminOrders = loadAdminOrders;
    window.refreshSelectedOrderFromApi = refreshSelectedOrderFromApi;

    document.addEventListener('DOMContentLoaded', wireOrderAdminUi);
})();
