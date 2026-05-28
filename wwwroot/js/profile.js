/**
 * Đồng bộ profile (tên, avatar, hình nền) giữa API và localStorage.
 */

function saveProfileToLocalStorage(profile) {
    if (!profile) return;
    if (profile.displayName != null && String(profile.displayName).trim()) {
        localStorage.setItem('userName', String(profile.displayName).trim());
    } else if (profile.username) {
        localStorage.setItem('userName', profile.username);
    }
    if (profile.avatarUrl != null && String(profile.avatarUrl).trim()) {
        localStorage.setItem('userAvatar', String(profile.avatarUrl).trim());
    } else {
        localStorage.removeItem('userAvatar');
    }
    if (profile.backgroundUrl != null && String(profile.backgroundUrl).trim()) {
        localStorage.setItem('userBackground', String(profile.backgroundUrl).trim());
    } else {
        localStorage.removeItem('userBackground');
    }
    if (profile.username) {
        localStorage.setItem('loginUsername', profile.username);
    }
}

function applyUserBackground() {
    var url = localStorage.getItem('userBackground');
    var body = document.body;
    if (!body) return;
    var theme = document.documentElement.getAttribute('data-theme') || 'light';
    var isDark = theme === 'dark';

    if (url && url.trim()) {
        var u = url.trim();
        if (!/^https?:\/\//i.test(u) && u.charAt(0) !== '/') {
            u = '/' + u.replace(/^\//, '');
        }
        var overlay = isDark
            ? 'linear-gradient(rgba(11, 18, 32, 0.82), rgba(11, 18, 32, 0.88))'
            : 'linear-gradient(rgba(244, 247, 251, 0.8), rgba(244, 247, 251, 0.9))';
        body.style.backgroundImage = overlay + ', url("' + u.replace(/"/g, '%22') + '")';
        body.style.backgroundSize = 'cover';
        body.style.backgroundPosition = 'center';
        body.style.backgroundAttachment = 'fixed';
        body.classList.add('has-user-background');
    } else {
        body.style.backgroundImage = '';
        body.style.backgroundSize = '';
        body.style.backgroundPosition = '';
        body.style.backgroundAttachment = '';
        body.classList.remove('has-user-background');
    }
}

/** Hình nền cho trang admin (nền sáng). */
function applyUserBackgroundAdmin() {
    var url = localStorage.getItem('userBackground');
    var body = document.body;
    if (!body) return;

    if (url && url.trim()) {
        var u = url.trim();
        if (!/^https?:\/\//i.test(u) && u.charAt(0) !== '/') {
            u = '/' + u.replace(/^\//, '');
        }
        body.style.backgroundImage = 'linear-gradient(rgba(244, 247, 251, 0.88), rgba(244, 247, 251, 0.92)), url("' + u.replace(/"/g, '%22') + '")';
        body.style.backgroundSize = 'cover';
        body.style.backgroundPosition = 'center';
        body.style.backgroundAttachment = 'fixed';
        body.classList.add('has-user-background');
    } else {
        body.style.backgroundImage = '';
        body.style.backgroundSize = '';
        body.style.backgroundPosition = '';
        body.style.backgroundAttachment = '';
        body.classList.remove('has-user-background');
    }
}

async function fetchAndCacheProfile() {
    if (!getToken() || typeof API_ENDPOINTS === 'undefined' || !API_ENDPOINTS.PROFILE_ME) {
        return null;
    }
    try {
        var profile = await apiGet(API_ENDPOINTS.PROFILE_ME);
        saveProfileToLocalStorage(profile);
        if (typeof syncUserAvatarButton === 'function') {
            syncUserAvatarButton();
        }
        return profile;
    } catch (e) {
        console.warn('fetchAndCacheProfile', e);
        return null;
    }
}

function resolvePublicUrl(url) {
    if (!url || !String(url).trim()) return '';
    var u = String(url).trim();
    if (/^https?:\/\//i.test(u) || u.startsWith('data:')) return u;
    if (u.charAt(0) === '/') {
        return window.location.origin + u;
    }
    return u;
}

async function uploadProfileImageFile(file) {
    var fd = new FormData();
    fd.append('file', file);
    var result = await apiPostFormData(API_ENDPOINTS.UPLOAD_PROFILE_IMAGE, fd);
    return result && result.url ? result.url : null;
}

window.addEventListener('store-theme-changed', function () {
    if (document.body && document.body.classList.contains('admin-page')) {
        applyUserBackgroundAdmin();
        return;
    }
    applyUserBackground();
});
