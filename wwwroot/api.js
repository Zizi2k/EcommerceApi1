// Utility functions cho API calls

/**
 * Lấy token từ localStorage
 */
function getToken() {
    return localStorage.getItem('userToken');
}

/** Payload JWT (phần giữa), không xác thực chữ ký — chỉ đọc role hiển thị UI. */
function parseJwtPayload(token) {
    if (!token || typeof token !== 'string') return null;
    try {
        const parts = token.split('.');
        if (parts.length < 2) return null;
        let base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
        while (base64.length % 4) base64 += '=';
        const json = atob(base64);
        return JSON.parse(json);
    } catch (e) {
        console.warn('parseJwtPayload', e);
        return null;
    }
}

function getJwtRoleClaim(payload) {
    if (!payload) return null;
    const longKey = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
    return payload.role || payload[longKey] || payload.Role || null;
}

/** Chỉ tài khoản admin (JWT role Admin) mới quản lý sản phẩm. */
function isAdminUser() {
    const payload = parseJwtPayload(getToken());
    if (!payload) return false;
    const r = getJwtRoleClaim(payload);
    if (r != null) {
        if (Array.isArray(r)) return r.indexOf('Admin') >= 0;
        if (r === 'Admin') return true;
    }
    const nameClaim = payload.unique_name || payload.name || payload.preferred_username
        || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'];
    if (nameClaim && String(nameClaim).toLowerCase() === 'admin') return true;
    return false;
}

/**
 * Kiểm tra xem user đã đăng nhập chưa
 * Nếu chưa thì redirect tới login
 */
function checkAuth() {
    const token = getToken();
    if (!token) {
        alert("Bạn cần đăng nhập trước!");
        window.location.href = 'login.html';
        return false;
    }
    return true;
}

/**
 * Hàm gọi API chung
 * @param {string} url - URL endpoint
 * @param {object} options - Các tuỳ chọn fetch
 */
async function apiCall(url, options = {}) {
    try {
        const token = getToken();
        
        // Thêm authorization header nếu có token
        if (token) {
            options.headers = options.headers || {};
            options.headers['Authorization'] = `Bearer ${token}`;
        }
        
        // Set content-type mặc định
        if (!options.headers) {
            options.headers = {};
        }
        // FormData: không set Content-Type để trình duyệt gửi kèm boundary multipart
        if (options.body != null && typeof options.body === 'string' && !options.headers['Content-Type']) {
            options.headers['Content-Type'] = 'application/json';
        }
        
        const response = await fetch(url, options);
        
        if (!response.ok) {
            let detail = '';
            try {
                const errText = await response.text();
                if (errText) {
                    try {
                        const errJson = JSON.parse(errText);
                        if (errJson.message) detail = errJson.message;
                        else if (errJson.title) detail = errJson.title;
                        else detail = errText;
                    } catch {
                        detail = errText;
                    }
                }
            } catch { /* ignore */ }
            let msg = detail ? detail : `HTTP error! status: ${response.status}`;
            if (response.status === 404 && url && /profile|auth\/profile|admin\/customers/i.test(url)) {
                msg = 'API chưa cập nhật (404). Dừng EcommerceApi trong Task Manager, chạy lại dotnet run --launch-profile https, rồi Ctrl+F5 trang admin.';
            }
            throw new Error(msg);
        }
        
        const text = await response.text();
        if (!text) {
            return null;
        }
        try {
            return JSON.parse(text);
        } catch {
            return text;
        }
    } catch (error) {
        console.error('API Error:', error);
        if (error instanceof TypeError && (error.message === 'Failed to fetch' || error.message === 'Load failed')) {
            throw new Error('Không kết nối được máy chủ API. Hãy mở Task Manager, dừng EcommerceApi (nếu có), chạy lại: dotnet run --launch-profile https — sau đó Ctrl+F5 trang này.');
        }
        throw error;
    }
}

/**
 * GET request
 */
async function apiGet(url) {
    return apiCall(url, { method: 'GET' });
}

/**
 * POST request
 */
async function apiPost(url, data) {
    return apiCall(url, {
        method: 'POST',
        body: JSON.stringify(data)
    });
}

/** POST multipart (upload file); không set Content-Type thủ công. */
async function apiPostFormData(url, formData) {
    return apiCall(url, {
        method: 'POST',
        body: formData
    });
}

/**
 * PUT request
 */
async function apiPut(url, data) {
    return apiCall(url, {
        method: 'PUT',
        body: JSON.stringify(data)
    });
}

/**
 * DELETE request
 */
async function apiDelete(url) {
    return apiCall(url, { method: 'DELETE' });
}
