// Lấy base URL động (tự động xác định port)
const API_BASE_URL = `${window.location.protocol}//${window.location.host}/api`;

// Các endpoint API
const API_ENDPOINTS = {
    LOGIN: `${API_BASE_URL}/auth/login`,
    PRODUCTS: `${API_BASE_URL}/Products`,
    CART: `${API_BASE_URL}/Cart`,
    CART_ADD: `${API_BASE_URL}/Cart/add`,
};

console.log('API Base URL:', API_BASE_URL);
