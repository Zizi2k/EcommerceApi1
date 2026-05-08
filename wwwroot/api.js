// Utility functions cho API calls

/**
 * Lấy token từ localStorage
 */
function getToken() {
    return localStorage.getItem('userToken');
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
        if (!options.headers['Content-Type'] && options.body) {
            options.headers['Content-Type'] = 'application/json';
        }
        
        const response = await fetch(url, options);
        
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        
        return await response.json();
    } catch (error) {
        console.error('API Error:', error);
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
