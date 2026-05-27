// Lấy base URL động (tự động xác định port)
const API_BASE_URL = `${window.location.protocol}//${window.location.host}/api`;

// Các endpoint API
const API_ENDPOINTS = {
    LOGIN: `${API_BASE_URL}/auth/login`,
    REGISTER: `${API_BASE_URL}/auth/register`,
    REGISTER_SEND_OTP: `${API_BASE_URL}/auth/register/send-otp`,
    REGISTER_VERIFY_OTP: `${API_BASE_URL}/auth/register/verify-otp`,
    PROFILE_ME: `${API_BASE_URL}/auth/profile/me`,
    PROFILE: `${API_BASE_URL}/auth/profile`,
    PROFILE_PASSWORD: `${API_BASE_URL}/auth/profile/password`,
    UPLOAD_PROFILE_IMAGE: `${API_BASE_URL}/Upload/profile-image`,
    PRODUCTS: `${API_BASE_URL}/Products`,
    UPLOAD_PRODUCT_IMAGE: `${API_BASE_URL}/Upload/product-image`,
    CATEGORIES: `${API_BASE_URL}/Categories`,
    CART: `${API_BASE_URL}/Cart`,
    CART_ADD: `${API_BASE_URL}/Cart/add`,
    CHECKOUT: `${API_BASE_URL}/Checkout`,
    ORDERS_MY: `${API_BASE_URL}/Orders/my`,
    ORDERS_CANCEL_REASONS: `${API_BASE_URL}/Orders/cancel-reasons`,
    ORDERS: `${API_BASE_URL}/Orders`,
    ADMIN_CUSTOMERS: `${API_BASE_URL}/auth/admin/customers`,
    ADMIN_ORDERS: `${API_BASE_URL}/Admin/orders`,
    ADMIN_ORDERS_REPORT_MONTHLY: `${API_BASE_URL}/Admin/orders/reports/monthly`,
    NOTIFICATIONS: `${API_BASE_URL}/Notifications`,
    NOTIFICATIONS_UNREAD: `${API_BASE_URL}/Notifications/unread-count`,
    NOTIFICATIONS_READ_ALL: `${API_BASE_URL}/Notifications/read-all`,
    PROMOTIONS: `${API_BASE_URL}/Promotions`,
    PROMOTIONS_ALL: `${API_BASE_URL}/Promotions/all`,
    PROMOTIONS_FLASH_SALE: `${API_BASE_URL}/Promotions/flash-sale`,
};

/** Danh mục cố định id 1–11 (khớp DB / migration). Dùng khi API không trả categoryName. */
const CATEGORY_CATALOG = [
    { id: 1, name: 'Laptop & Máy Tính' },
    { id: 2, name: 'Smartphone' },
    { id: 3, name: 'Tai Nghe' },
    { id: 4, name: 'Camera & Photo' },
    { id: 5, name: 'Phím & Chuột' },
    { id: 6, name: 'Màn Hình & Display' },
    { id: 7, name: 'Router & Network' },
    { id: 8, name: 'Pin & Sạc' },
    { id: 9, name: 'Máy Chiếu' },
    { id: 10, name: 'Wearable' },
    { id: 11, name: 'Phụ Kiện' },
];

function resolveCategoryName(categoryId, categoryNameFromApi) {
    const fromApi = categoryNameFromApi != null ? String(categoryNameFromApi).trim() : '';
    if (fromApi) return fromApi;
    const id = parseInt(categoryId, 10);
    if (!Number.isFinite(id) || id < 1) return '';
    const row = CATEGORY_CATALOG.find(function (c) { return c.id === id; });
    return row ? row.name : '';
}

console.log('API Base URL:', API_BASE_URL);
