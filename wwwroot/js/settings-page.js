function showSettingsAlert(text, isError) {
    var el = document.getElementById('settings-alert');
    if (!el) return;
    if (!isError && window.AppNotify && typeof window.AppNotify.success === 'function') {
        window.AppNotify.success(text);
    }
    el.textContent = text;
    el.className = 'settings-alert ' + (isError ? 'error' : 'success');
    el.hidden = false;
    setTimeout(function () {
        el.hidden = true;
    }, 5000);
}

function updateAvatarPreview(url) {
    var img = document.getElementById('settings-avatar-preview');
    var fb = document.getElementById('settings-avatar-fallback');
    if (!img || !fb) return;
    var u = resolvePublicUrl(url);
    if (!u) {
        img.style.display = 'none';
        fb.style.display = 'flex';
        img.removeAttribute('src');
        return;
    }
    img.onload = function () {
        img.style.display = 'block';
        fb.style.display = 'none';
    };
    img.onerror = function () {
        img.style.display = 'none';
        fb.style.display = 'flex';
    };
    img.src = u;
}

function updateBgPreview(url) {
    var wrap = document.getElementById('bg-preview-wrap');
    var box = document.getElementById('bg-preview');
    if (!wrap || !box) return;
    var u = resolvePublicUrl(url);
    if (!u) {
        wrap.hidden = true;
        box.style.backgroundImage = '';
        return;
    }
    wrap.hidden = false;
    box.style.backgroundImage = 'url("' + u.replace(/"/g, '%22') + '")';
}

function fillFormFromProfile(profile) {
    if (!profile) return;
    var nameEl = document.getElementById('displayName');
    var avatarEl = document.getElementById('avatarUrl');
    var bgEl = document.getElementById('backgroundUrl');
    var sidebarName = document.getElementById('settings-sidebar-name');
    var loginId = document.getElementById('settings-login-id');

    if (nameEl) nameEl.value = profile.displayName || profile.username || '';
    if (avatarEl) avatarEl.value = profile.avatarUrl || '';
    if (bgEl) bgEl.value = profile.backgroundUrl || '';
    if (sidebarName) sidebarName.textContent = profile.displayName || profile.username || 'Tài khoản';
    if (loginId) {
        var u = profile.username || '';
        loginId.textContent = u ? (u.indexOf('@') >= 0 ? u : '@' + u) : '—';
    }

    updateAvatarPreview(profile.avatarUrl);
    updateBgPreview(profile.backgroundUrl);
}

function pickAvatarFile() {
    var input = document.getElementById('avatarFile');
    if (input) input.click();
}

function pickBackgroundFile() {
    var input = document.getElementById('backgroundFile');
    if (input) input.click();
}

async function persistProfileAfterUpload(uploadedField, uploadedUrl) {
    if (!uploadedUrl) {
        throw new Error('Server không trả về URL ảnh hợp lệ.');
    }

    var displayNameEl = document.getElementById('displayName');
    var avatarUrlEl = document.getElementById('avatarUrl');
    var backgroundUrlEl = document.getElementById('backgroundUrl');

    var payload = {
        displayName: displayNameEl ? displayNameEl.value.trim() : '',
        avatarUrl: avatarUrlEl ? avatarUrlEl.value.trim() : '',
        backgroundUrl: backgroundUrlEl ? backgroundUrlEl.value.trim() : ''
    };

    if (uploadedField === 'avatar') {
        payload.avatarUrl = uploadedUrl;
    } else if (uploadedField === 'background') {
        payload.backgroundUrl = uploadedUrl;
    }

    var profile = await apiPut(API_ENDPOINTS.PROFILE, payload);
    saveProfileToLocalStorage(profile);
    fillFormFromProfile(profile);
    syncUserAvatarButton();
    applyUserBackground();
}

async function saveProfile(event) {
    event.preventDefault();
    var payload = {
        displayName: document.getElementById('displayName').value.trim(),
        avatarUrl: document.getElementById('avatarUrl').value.trim(),
        backgroundUrl: document.getElementById('backgroundUrl').value.trim()
    };
    try {
        var profile = await apiPut(API_ENDPOINTS.PROFILE, payload);
        saveProfileToLocalStorage(profile);
        fillFormFromProfile(profile);
        syncUserAvatarButton();
        applyUserBackground();
        showSettingsAlert('Đã lưu hồ sơ.', false);
    } catch (e) {
        showSettingsAlert(e.message || 'Không lưu được hồ sơ.', true);
    }
}

async function savePassword(event) {
    event.preventDefault();
    var current = document.getElementById('currentPassword').value;
    var newer = document.getElementById('newPassword').value;
    var confirm = document.getElementById('confirmPassword').value;
    if (newer !== confirm) {
        showSettingsAlert('Mật khẩu mới không khớp.', true);
        return;
    }
    try {
        await apiPut(API_ENDPOINTS.PROFILE_PASSWORD, {
            currentPassword: current,
            newPassword: newer
        });
        document.getElementById('form-password').reset();
        showSettingsAlert('Đã đổi mật khẩu.', false);
    } catch (e) {
        showSettingsAlert(e.message || 'Không đổi được mật khẩu.', true);
    }
}

document.addEventListener('DOMContentLoaded', async function () {
    if (!getToken()) {
        window.location.href = 'login.html';
        return;
    }
    document.body.classList.add('app-user-logged-in');
    initializeHeader();

    var profile = null;
    try {
        profile = await fetchAndCacheProfile();
    } catch (e) {
        console.warn('load profile', e);
    }
    if (profile) {
        fillFormFromProfile(profile);
    } else {
        fillFormFromProfile({
            username: localStorage.getItem('loginUsername') || '',
            displayName: localStorage.getItem('userName') || '',
            avatarUrl: localStorage.getItem('userAvatar') || '',
            backgroundUrl: localStorage.getItem('userBackground') || ''
        });
        if (getToken()) {
            showSettingsAlert(
                'Không tải được hồ sơ từ server (có thể API chưa chạy). Bạn vẫn xem dữ liệu đã lưu trên trình duyệt; lưu hồ sơ cần chạy lại EcommerceApi.',
                true
            );
        }
    }
    applyUserBackground();

    var avatarInput = document.getElementById('avatarFile');
    if (avatarInput) {
        avatarInput.addEventListener('change', async function () {
            var file = avatarInput.files && avatarInput.files[0];
            if (!file) return;
            var status = document.getElementById('avatar-upload-status');
            if (status) status.textContent = 'Đang tải lên...';
            try {
                var url = await uploadProfileImageFile(file);
                await persistProfileAfterUpload('avatar', url);
                if (status) status.textContent = 'Đã cập nhật ảnh đại diện.';
            } catch (e) {
                if (status) status.textContent = '';
                showSettingsAlert(e.message || 'Tải ảnh thất bại.', true);
            }
            avatarInput.value = '';
        });
    }

    var bgInput = document.getElementById('backgroundFile');
    if (bgInput) {
        bgInput.addEventListener('change', async function () {
            var file = bgInput.files && bgInput.files[0];
            if (!file) return;
            var status = document.getElementById('bg-upload-status');
            if (status) status.textContent = 'Đang tải lên...';
            try {
                var url = await uploadProfileImageFile(file);
                await persistProfileAfterUpload('background', url);
                if (status) status.textContent = 'Đã cập nhật hình nền.';
            } catch (e) {
                if (status) status.textContent = '';
                showSettingsAlert(e.message || 'Tải ảnh thất bại.', true);
            }
            bgInput.value = '';
        });
    }

    var bgUrlInput = document.getElementById('backgroundUrl');
    if (bgUrlInput) {
        bgUrlInput.addEventListener('input', function () {
            updateBgPreview(bgUrlInput.value);
        });
    }
});
