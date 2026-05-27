(function () {
    var ordersCache = [];
    var selectedOrderId = null;
    var selectedRating = 0;
    var orderAutoRefreshTimer = null;
    var CANCEL_REASONS = [
        'Đặt nhầm sản phẩm',
        'Muốn thay đổi sản phẩm khác',
        'Thay đổi số lượng sản phẩm',
        'Không còn nhu cầu mua hàng',
        'Tìm được giá tốt hơn ở nơi khác',
        'Thời gian giao hàng quá lâu',
        'Sai thông tin đơn hàng',
        'Muốn thay đổi địa chỉ nhận hàng',
        'Muốn thay đổi phương thức thanh toán',
        'Sản phẩm không đúng nhu cầu',
        'Đặt trùng đơn hàng',
        'Không đủ khả năng thanh toán',
        'Lỗi khi đặt hàng',
        'Không liên lạc được với người bán',
        'Muốn đặt lại đơn mới',
        'Hết hàng / không còn sản phẩm',
        'Lý do cá nhân',
        'Khác'
    ];

    function escHtml(s) {
        return String(s == null ? '' : s)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/"/g, '&quot;');
    }

    function fmtMoney(v) {
        return Number(v || 0).toLocaleString('vi-VN') + ' VNĐ';
    }

    function fmtDate(iso) {
        if (!iso) return '—';
        try {
            return new Date(iso).toLocaleString('vi-VN', { dateStyle: 'short', timeStyle: 'short' });
        } catch (e) {
            return iso;
        }
    }

    function statusStep(status) {
        if (status === 'Cancelled') return 0;
        if (status === 'Delivered') return 3;
        if (status === 'Delivering') return 2;
        return 1;
    }

    function renderOrderSelect() {
        var sel = document.getElementById('my-order-select');
        var count = document.getElementById('my-orders-count');
        if (!sel || !count) return;

        sel.innerHTML = '<option value="">-- Chọn đơn hàng --</option>' + ordersCache.map(function (o) {
            return '<option value="' + o.id + '">#' + o.id + ' · ' + fmtDate(o.createdAtUtc) + ' · ' + fmtMoney(o.totalAmount) + '</option>';
        }).join('');
        count.textContent = ordersCache.length + ' đơn';
        if (selectedOrderId) sel.value = String(selectedOrderId);
    }

    function setTracking(status) {
        var step = statusStep(status);
        ['prep', 'ship', 'done'].forEach(function (id, idx) {
            var node = document.getElementById('track-' + id);
            if (!node) return;
            node.classList.remove('is-on', 'is-done');
            if (idx + 1 < step) node.classList.add('is-done');
            if (idx + 1 === step) node.classList.add('is-on');
        });
    }

    function renderStars(rating) {
        var row = document.getElementById('my-order-star-row');
        if (!row) return;
        row.innerHTML = '';
        for (var i = 1; i <= 5; i++) {
            (function (star) {
                var btn = document.createElement('button');
                btn.type = 'button';
                btn.className = 'my-order-star-btn' + (star <= rating ? ' is-on' : '');
                btn.innerHTML = '<i class="fa-solid fa-star"></i>';
                btn.addEventListener('click', function () {
                    selectedRating = star;
                    renderStars(selectedRating);
                });
                row.appendChild(btn);
            })(i);
        }
    }

    function showOrderMessage(msg, isErr) {
        var box = document.getElementById('my-order-msg');
        if (!box) return;
        if (!msg) {
            box.style.display = 'none';
            box.className = 'my-order-msg';
            box.textContent = '';
            return;
        }
        box.style.display = 'block';
        box.className = 'my-order-msg ' + (isErr ? 'err' : 'ok');
        box.textContent = msg;
    }

    function renderOrderDetail(order) {
        var empty = document.getElementById('my-order-empty');
        var pane = document.getElementById('my-order-detail');
        if (!empty || !pane) return;
        showOrderMessage('', false);

        if (!order) {
            empty.style.display = 'block';
            pane.style.display = 'none';
            return;
        }

        empty.style.display = 'none';
        pane.style.display = 'block';
        document.getElementById('my-order-id').textContent = '#' + order.id;
        document.getElementById('my-order-time').textContent = fmtDate(order.createdAtUtc);
        document.getElementById('my-order-status').textContent = order.statusLabel;
        document.getElementById('my-order-total').textContent = fmtMoney(order.totalAmount);
        document.getElementById('my-order-payment').textContent = order.paymentMethod || '—';
        document.getElementById('my-order-shipping').textContent = order.shippingAddress || '—';
        setTracking(order.status);

        var items = document.getElementById('my-order-items');
        if (items) {
            items.innerHTML = (order.items || []).map(function (it) {
                return '<li><span>' + escHtml(it.productName) + ' x ' + it.quantity + '</span><strong>' + fmtMoney(it.lineTotal) + '</strong></li>';
            }).join('');
        }

        selectedRating = order.customerRating || 0;
        renderStars(selectedRating);
        var note = document.getElementById('my-order-review-note');
        if (note) note.value = order.customerReviewNote || '';

        var reviewBox = document.getElementById('my-order-review-box');
        if (reviewBox) reviewBox.classList.toggle('is-locked', !order.canReview);
        var reviewHint = document.getElementById('my-order-review-hint');
        if (reviewHint) {
            if (order.canReview && order.customerRating) {
                reviewHint.textContent = 'Bạn có thể cập nhật lại đánh giá cho đơn đã giao.';
            } else if (order.canReview) {
                reviewHint.textContent = 'Bạn có thể đánh giá trải nghiệm đơn hàng này.';
            } else {
                reviewHint.textContent = 'Đơn chưa giao thành công, chưa thể đánh giá.';
            }
        }
        var reviewed = document.getElementById('my-order-reviewed');
        if (reviewed) {
            if (order.customerRating) {
                reviewed.style.display = 'block';
                reviewed.innerHTML = 'Đã đánh giá: <strong>' + order.customerRating + '/5</strong>' +
                    (order.customerReviewNote ? ' - ' + escHtml(order.customerReviewNote) : '') +
                    (order.customerReviewedAtUtc ? '<br><small>' + fmtDate(order.customerReviewedAtUtc) + '</small>' : '');
            } else {
                reviewed.style.display = 'none';
                reviewed.innerHTML = '';
            }
        }

        var cancelBox = document.getElementById('my-order-cancel-box');
        if (cancelBox) cancelBox.classList.toggle('is-locked', !order.canCancel);
        var cancelHint = document.getElementById('my-order-cancel-hint');
        if (cancelHint) {
            if (order.status === 'Cancelled') {
                cancelHint.textContent = 'Đơn hàng đã được hủy.';
            } else if (order.canCancel) {
                cancelHint.textContent = 'Bạn có thể gửi yêu cầu hủy đơn và chọn lý do.';
            } else if (order.cancelReason) {
                cancelHint.textContent = 'Bạn đã gửi yêu cầu hủy đơn, vui lòng chờ admin xử lý.';
            } else {
                cancelHint.textContent = 'Đơn đã giao nên không thể hủy.';
            }
        }
        var cancelReasonSel = document.getElementById('my-order-cancel-reason');
        fillCancelReasonSelect(order.cancelReason || '');
        var cancelNote = document.getElementById('my-order-cancel-note');
        if (cancelNote) cancelNote.value = order.cancelNote || '';
        var cancelSent = document.getElementById('my-order-cancel-sent');
        if (cancelSent) {
            if (order.status === 'Cancelled' && order.cancelReason) {
                cancelSent.style.display = 'block';
                cancelSent.innerHTML = 'Đơn đã hủy. Lý do: <strong>' + escHtml(order.cancelReason) + '</strong>' +
                    (order.cancelRequestedAtUtc ? '<br><small>' + fmtDate(order.cancelRequestedAtUtc) + '</small>' : '');
            } else if (order.cancelReason) {
                cancelSent.style.display = 'block';
                cancelSent.innerHTML = 'Đã gửi yêu cầu hủy: <strong>' + escHtml(order.cancelReason) + '</strong>' +
                    (order.cancelNote ? ' - ' + escHtml(order.cancelNote) : '') +
                    (order.cancelRequestedAtUtc ? '<br><small>' + fmtDate(order.cancelRequestedAtUtc) + '</small>' : '');
            } else {
                cancelSent.style.display = 'none';
                cancelSent.innerHTML = '';
            }
        }
    }

    async function loadMyOrders() {
        var loading = document.getElementById('my-orders-loading');
        var wrap = document.getElementById('my-orders-wrap');
        var noOrders = document.getElementById('my-orders-no-data');
        var prevSelected = selectedOrderId;
        if (loading) loading.style.display = 'block';
        if (wrap) wrap.style.display = 'none';
        if (noOrders) noOrders.style.display = 'none';
        try {
            var list = await apiGet(API_ENDPOINTS.ORDERS_MY);
            ordersCache = Array.isArray(list) ? list : [];
            if (loading) loading.style.display = 'none';
            if (!ordersCache.length) {
                if (noOrders) noOrders.style.display = 'block';
                selectedOrderId = null;
                return;
            }
            if (wrap) wrap.style.display = 'block';
            if (prevSelected && ordersCache.some(function (o) { return o.id === prevSelected; })) {
                selectedOrderId = prevSelected;
            } else {
                selectedOrderId = ordersCache[0].id;
            }
            renderOrderSelect();
            var selected = ordersCache.find(function (o) { return o.id === selectedOrderId; }) || ordersCache[0];
            renderOrderDetail(selected);
        } catch (e) {
            if (loading) loading.style.display = 'none';
            if (noOrders) {
                noOrders.style.display = 'block';
                noOrders.textContent = 'Không tải được đơn hàng: ' + (e.message || e);
            }
        }
    }

    function startAutoRefreshOrders() {
        stopAutoRefreshOrders();
        orderAutoRefreshTimer = setInterval(function () {
            if (!getToken()) return;
            loadMyOrders();
        }, 10000);
    }

    function stopAutoRefreshOrders() {
        if (orderAutoRefreshTimer) {
            clearInterval(orderAutoRefreshTimer);
            orderAutoRefreshTimer = null;
        }
    }

    function onSelectChange() {
        var sel = document.getElementById('my-order-select');
        var id = sel ? parseInt(sel.value, 10) : NaN;
        selectedOrderId = isNaN(id) ? null : id;
        var order = ordersCache.find(function (x) { return x.id === selectedOrderId; });
        renderOrderDetail(order || null);
    }

    async function submitReview() {
        if (!selectedOrderId) {
            showOrderMessage('Vui lòng chọn đơn hàng.', true);
            return;
        }
        var order = ordersCache.find(function (x) { return x.id === selectedOrderId; });
        if (!order || !order.canReview) {
            showOrderMessage('Chỉ đánh giá khi đơn ở trạng thái đã giao.', true);
            return;
        }
        if (selectedRating < 1 || selectedRating > 5) {
            showOrderMessage('Vui lòng chọn số sao từ 1 đến 5.', true);
            return;
        }
        var note = document.getElementById('my-order-review-note');
        try {
            await apiPut(API_ENDPOINTS.ORDERS + '/' + selectedOrderId + '/review', {
                rating: selectedRating,
                note: note ? note.value.trim() : ''
            });
            showOrderMessage('Đã gửi đánh giá. Cảm ơn bạn!', false);
            if (typeof refreshNotifications === 'function') refreshNotifications();
            await loadMyOrders();
            var sel = document.getElementById('my-order-select');
            if (sel) {
                sel.value = String(selectedOrderId);
                onSelectChange();
            }
        } catch (e) {
            showOrderMessage(e.message || 'Gửi đánh giá thất bại.', true);
        }
    }

    async function submitCancel() {
        if (!selectedOrderId) {
            showOrderMessage('Vui lòng chọn đơn hàng.', true);
            return;
        }
        var order = ordersCache.find(function (x) { return x.id === selectedOrderId; });
        if (!order || !order.canCancel) {
            showOrderMessage('Đơn hiện không thể gửi yêu cầu hủy.', true);
            return;
        }
        var reasonSel = document.getElementById('my-order-cancel-reason');
        var noteEl = document.getElementById('my-order-cancel-note');
        var reason = getSelectedCancelReason();
        if (!reason) {
            showOrderMessage('Vui lòng chọn lý do hủy đơn.', true);
            if (reasonSel) reasonSel.focus();
            return;
        }
        try {
            await apiPut(API_ENDPOINTS.ORDERS + '/' + selectedOrderId + '/cancel', {
                reason: reason,
                note: noteEl ? noteEl.value.trim() : ''
            });
            showOrderMessage('Đã gửi yêu cầu hủy đơn cho admin.', false);
            if (typeof refreshNotifications === 'function') refreshNotifications();
            await loadMyOrders();
            var sel = document.getElementById('my-order-select');
            if (sel) {
                sel.value = String(selectedOrderId);
                onSelectChange();
            }
        } catch (e) {
            showOrderMessage(e.message || 'Gửi yêu cầu hủy thất bại.', true);
        }
    }

    function getSelectedCancelReason() {
        var sel = document.getElementById('my-order-cancel-reason');
        if (!sel || sel.value === '') return '';
        var idx = parseInt(sel.value, 10);
        if (!isNaN(idx) && idx >= 0 && idx < CANCEL_REASONS.length) {
            return CANCEL_REASONS[idx];
        }
        return sel.value.trim();
    }

    function fillCancelReasonSelect(selectedValue) {
        var cancelReasonSel = document.getElementById('my-order-cancel-reason');
        if (!cancelReasonSel) return;
        var list = CANCEL_REASONS.length ? CANCEL_REASONS : [];
        cancelReasonSel.innerHTML = '<option value="">-- Chọn lý do hủy --</option>' +
            list.map(function (r, i) {
                return '<option value="' + i + '">' + escHtml(r) + '</option>';
            }).join('');
        if (selectedValue) {
            var idx = list.indexOf(selectedValue);
            if (idx >= 0) {
                cancelReasonSel.value = String(idx);
            } else {
                var opt = document.createElement('option');
                opt.value = String(list.length);
                opt.textContent = selectedValue;
                opt.selected = true;
                cancelReasonSel.appendChild(opt);
                CANCEL_REASONS.push(selectedValue);
            }
        }
    }

    async function loadCancelReasons() {
        try {
            var list = await apiGet(API_ENDPOINTS.ORDERS_CANCEL_REASONS);
            if (Array.isArray(list) && list.length) CANCEL_REASONS = list;
        } catch (e) {
            console.warn('loadCancelReasons', e);
        }
        fillCancelReasonSelect();
    }

    window.initAccountOrders = async function () {
        await loadCancelReasons();
        var sel = document.getElementById('my-order-select');
        if (sel && !sel._wired) {
            sel._wired = true;
            sel.addEventListener('change', onSelectChange);
        }
        var refresh = document.getElementById('btn-refresh-my-orders');
        if (refresh && !refresh._wired) {
            refresh._wired = true;
            refresh.addEventListener('click', loadMyOrders);
        }
        var submit = document.getElementById('btn-submit-my-review');
        if (submit && !submit._wired) {
            submit._wired = true;
            submit.addEventListener('click', submitReview);
        }
        var cancelBtn = document.getElementById('btn-submit-my-cancel');
        if (cancelBtn && !cancelBtn._wired) {
            cancelBtn._wired = true;
            cancelBtn.addEventListener('click', submitCancel);
        }
        if (!document._orderVisibilityWired) {
            document._orderVisibilityWired = true;
            document.addEventListener('visibilitychange', function () {
                if (document.visibilityState === 'visible') {
                    loadMyOrders();
                    startAutoRefreshOrders();
                } else {
                    stopAutoRefreshOrders();
                }
            });
        }
        startAutoRefreshOrders();
        loadMyOrders();
    };
})();
