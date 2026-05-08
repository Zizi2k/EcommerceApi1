// Header functions

/**
 * Initialize header on page load
 */
function initializeHeader() {
    const token = getToken();
    const userBtn = document.getElementById('user-btn');
    const loginNav = document.getElementById('login-nav');
    
    if (token && userBtn) {
        userBtn.style.display = 'block';
        if (loginNav) loginNav.style.display = 'none';
    } else if (userBtn) {
        userBtn.style.display = 'none';
        if (loginNav) loginNav.style.display = 'block';
    }

    // Setup event listeners
    setupHeaderListeners();
}

/**
 * Setup header event listeners
 */
function setupHeaderListeners() {
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
    });

    // Menu toggle for mobile
    const menuToggle = document.querySelector('.menu-toggle');
    const navMenu = document.querySelector('.nav-menu');
    
    if (menuToggle && navMenu) {
        menuToggle.addEventListener('click', function() {
            navMenu.classList.toggle('show');
        });
    }

    // Close mobile menu when clicking a link
    if (navMenu) {
        navMenu.addEventListener('click', function(e) {
            if (e.target.tagName === 'A' || e.target.tagName === 'BUTTON') {
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
    window.location.href = 'admin.html';
}

// Initialize on DOM ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeHeader);
} else {
    initializeHeader();
}
