// Header functions

let headerListenersAttached = false;

/**
 * Cập nhật nút tài khoản: avatar + tên từ localStorage (sau đăng nhập).
 */
function syncUserAvatarButton() {
    const userBtn = document.getElementById('user-btn');
    if (!userBtn) return;

    const avatarUrl = localStorage.getItem('userAvatar');
    const userName = localStorage.getItem('userName');
    const img = userBtn.querySelector('.user-avatar-img');
    const fallback = userBtn.querySelector('.user-avatar-fallback');
    const label = userBtn.querySelector('.user-btn-text');

    if (label) {
        label.textContent = (userName && userName.trim()) ? userName.trim() : 'Tài khoản';
    }

    if (!img || !fallback) return;

    userBtn.classList.remove('has-avatar');
    img.removeAttribute('src');
    img.alt = '';

    if (!avatarUrl || !avatarUrl.trim()) {
        return;
    }

    var url = avatarUrl.trim();
    img.onload = function () {
        userBtn.classList.add('has-avatar');
    };
    img.onerror = function () {
        userBtn.classList.remove('has-avatar');
        img.removeAttribute('src');
        img.alt = '';
    };
    img.src = url;
    img.alt = (userName && userName.trim()) ? userName.trim() : 'Tài khoản';
    if (img.complete && img.naturalHeight > 0) {
        userBtn.classList.add('has-avatar');
    }
}

/**
 * Initialize header on page load
 */
function initializeHeader() {
    const token = getToken();
    const userBtn = document.getElementById('user-btn');
    const loginNav = document.getElementById('login-nav');
    
    if (token && userBtn) {
        userBtn.style.display = 'inline-flex';
        syncUserAvatarButton();
        if (loginNav) loginNav.style.display = 'none';
    } else if (userBtn) {
        userBtn.style.display = 'none';
        userBtn.classList.remove('has-avatar');
        if (loginNav) loginNav.style.display = 'inline-flex';
    }

    document.body.classList.toggle('app-user-logged-in', !!token);

    document.querySelectorAll('.nav-admin-only, [data-admin-only]').forEach(function (el) {
        el.style.display = token && typeof isAdminUser === 'function' && isAdminUser() ? '' : 'none';
    });

    // Setup event listeners
    setupHeaderListeners();
}

/**
 * Setup header event listeners
 */
function setupHeaderListeners() {
    if (headerListenersAttached) {
        return;
    }
    headerListenersAttached = true;

    // User dropdown
    const userBtn = document.getElementById('user-btn');
    const dropdownMenu = document.getElementById('user-dropdown');
    
    if (userBtn && dropdownMenu) {
        userBtn.addEventListener('click', function(e) {
            e.stopPropagation();
            dropdownMenu.classList.toggle('show');
        });
    }

    // Close dropdown when clicking outside
    document.addEventListener('click', function(e) {
        if (dropdownMenu && !dropdownMenu.contains(e.target) && userBtn && !userBtn.contains(e.target)) {
            dropdownMenu.classList.remove('show');
        }
        const productNav = document.querySelector('.nav-dropdown--products');
        if (productNav && !productNav.contains(e.target)) {
            productNav.classList.remove('open');
            const pt = productNav.querySelector('.nav-dropdown-toggle');
            if (pt) pt.setAttribute('aria-expanded', 'false');
        }
    });

    // Sản phẩm: mở submenu (xem cửa hàng / module quản lý)
    const productNav = document.querySelector('.nav-dropdown--products');
    const productToggle = productNav && productNav.querySelector('.nav-dropdown-toggle');
    if (productToggle && productNav) {
        productToggle.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            const open = productNav.classList.toggle('open');
            productToggle.setAttribute('aria-expanded', open ? 'true' : 'false');
        });
    }

    // Menu toggle for mobile
    const menuToggle = document.querySelector('.menu-toggle');
    const navMenu = document.querySelector('.nav-menu');
    
    if (menuToggle && navMenu) {
        menuToggle.addEventListener('click', function() {
            navMenu.classList.toggle('show');
        });
    }

    // Close mobile menu when clicking a link (không đóng khi bấm mở submenu Sản phẩm)
    if (navMenu) {
        navMenu.addEventListener('click', function(e) {
            if (e.target.closest('.nav-dropdown-toggle')) {
                return;
            }
            const productNav = document.querySelector('.nav-dropdown--products');
            if (productNav) {
                productNav.classList.remove('open');
                const pt = productNav.querySelector('.nav-dropdown-toggle');
                if (pt) pt.setAttribute('aria-expanded', 'false');
            }
            if (e.target.closest('a') || e.target.closest('button')) {
                navMenu.classList.remove('show');
            }
        });
    }
}

/**
 * Logout user
 */
function logout() {
    localStorage.removeItem('userToken');
    localStorage.removeItem('userAvatar');
    localStorage.removeItem('userName');
    document.body.classList.remove('app-user-logged-in');
    alert("Đã đăng xuất thành công!");
    window.location.href = 'login.html';
}

/**
 * Navigate to login
 */
function navigateToLogin() {
    window.location.href = 'login.html';
}

/**
 * Navigate to home
 */
function navigateToHome() {
    window.location.href = 'index.html';
}

/**
 * Navigate to products
 */
function navigateToProducts() {
    window.location.href = 'index.html#products';
}

/**
 * Navigate to cart
 */
function navigateToCart() {
    if (!checkAuth()) return;
    window.location.href = 'cart.html';
}

/**
 * Navigate to admin
 */
function navigateToAdmin() {
    if (typeof isAdminUser === 'function' && !isAdminUser()) {
        alert('Chỉ tài khoản admin mới vào được trang quản lý.');
        return;
    }
    window.location.href = 'admin.html';
}

/** Trang khu vực thành viên (sau đăng nhập) */
function navigateToAccount() {
    window.location.href = 'account.html';
}

// Initialize on DOM ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeHeader);
} else {
    initializeHeader();
}
