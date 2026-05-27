(function () {
    const PRICE_MAX = 100000000;
    const PRICE_STEP = 100000;

    const els = {
        minInput: () => document.getElementById('min-price'),
        maxInput: () => document.getElementById('max-price'),
        minRange: () => document.getElementById('price-range-min'),
        maxRange: () => document.getElementById('price-range-max'),
        display: () => document.getElementById('price-range-display'),
        fill: () => document.getElementById('price-range-fill'),
        chips: () => document.getElementById('filter-active-chips'),
        search: () => document.getElementById('search-input'),
        category: () => document.getElementById('category-filter'),
        applyBtn: () => document.getElementById('btn-apply-filters'),
    };

    const fmt = (n) =>
        new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 }).format(n);

    function clamp(val, min, max) {
        return Math.min(Math.max(val, min), max);
    }

    function parsePrice(val) {
        const n = parseInt(String(val).replace(/\D/g, ''), 10);
        return isNaN(n) ? 0 : clamp(n, 0, PRICE_MAX);
    }

    function getMinMax() {
        let min = parsePrice(els.minInput()?.value);
        let max = parsePrice(els.maxInput()?.value);
        if (max === 0 && !els.maxInput()?.value) max = PRICE_MAX;
        if (min > max) [min, max] = [max, min];
        return { min, max };
    }

    function syncUi(fromRange) {
        const minR = els.minRange();
        const maxR = els.maxRange();
        const minI = els.minInput();
        const maxI = els.maxInput();
        const fill = els.fill();
        const display = els.display();
        if (!minR || !maxR) return;

        let min, max;
        if (fromRange) {
            min = parseInt(minR.value, 10);
            max = parseInt(maxR.value, 10);
            if (min > max) {
                if (document.activeElement === minR) max = min;
                else min = max;
            }
            if (minI) minI.value = min === 0 ? '' : String(min);
            if (maxI) maxI.value = max >= PRICE_MAX ? '' : String(max);
            minR.value = min;
            maxR.value = max;
        } else {
            ({ min, max } = getMinMax());
            minR.value = min;
            maxR.value = max;
        }

        const pctMin = (min / PRICE_MAX) * 100;
        const pctMax = (max / PRICE_MAX) * 100;
        if (fill) {
            fill.style.left = pctMin + '%';
            fill.style.width = (pctMax - pctMin) + '%';
        }
        if (display) {
            const maxLabel = max >= PRICE_MAX ? fmt(PRICE_MAX) + '+' : fmt(max);
            display.textContent = fmt(min) + ' — ' + maxLabel;
        }
        clearPresetActive();
    }

    function clearPresetActive() {
        document.querySelectorAll('.product-filter__preset').forEach((b) => b.classList.remove('is-active'));
    }

    function setPreset(min, max) {
        const minR = els.minRange();
        const maxR = els.maxRange();
        const minI = els.minInput();
        const maxI = els.maxInput();
        if (minR) minR.value = min;
        if (maxR) maxR.value = max;
        if (minI) minI.value = min === 0 ? '' : String(min);
        if (maxI) maxI.value = max >= PRICE_MAX ? '' : String(max);
        syncUi(true);
    }

    function updateChips() {
        const container = els.chips();
        if (!container) return;

        const chips = [];
        const search = (els.search()?.value || '').trim();
        const catSel = els.category();
        const catText = catSel?.selectedOptions?.[0]?.text;
        const catVal = catSel?.value;
        const { min, max } = getMinMax();

        if (search) chips.push({ key: 'search', label: 'Tìm: ' + search });
        if (catVal && catText) chips.push({ key: 'category', label: catText });
        if (min > 0 || max < PRICE_MAX) {
            chips.push({
                key: 'price',
                label: 'Giá: ' + fmt(min) + ' – ' + (max >= PRICE_MAX ? 'trở lên' : fmt(max)),
            });
        }

        container.innerHTML = '';
        if (!chips.length) {
            container.setAttribute('hidden', '');
            return;
        }
        container.removeAttribute('hidden');

        chips.forEach((chip) => {
            const el = document.createElement('span');
            el.className = 'product-filter__chip';
            el.innerHTML =
                chip.label +
                ' <button type="button" aria-label="Xóa bộ lọc" data-chip="' +
                chip.key +
                '">&times;</button>';
            el.querySelector('button').addEventListener('click', () => removeChip(chip.key));
            container.appendChild(el);
        });
    }

    function removeChip(key) {
        if (key === 'search' && els.search()) els.search().value = '';
        if (key === 'category' && els.category()) els.category().value = '';
        if (key === 'price') setPreset(0, PRICE_MAX);
        if (typeof applyFilters === 'function') applyFilters();
    }

    function resetUi() {
        if (els.search()) els.search().value = '';
        if (els.category()) els.category().value = '';
        setPreset(0, PRICE_MAX);
        updateChips();
        clearPresetActive();
    }

    function init() {
        const minR = els.minRange();
        const maxR = els.maxRange();
        if (!minR || !maxR) return;

        minR.max = maxR.max = PRICE_MAX;
        minR.step = maxR.step = PRICE_STEP;
        setPreset(0, PRICE_MAX);

        minR.addEventListener('input', () => syncUi(true));
        maxR.addEventListener('input', () => syncUi(true));

        els.minInput()?.addEventListener('change', () => syncUi(false));
        els.maxInput()?.addEventListener('change', () => syncUi(false));

        document.querySelectorAll('.product-filter__preset').forEach((btn) => {
            btn.addEventListener('click', () => {
                const min = parseInt(btn.dataset.min, 10) || 0;
                const max = parseInt(btn.dataset.max, 10) || PRICE_MAX;
                setPreset(min, max);
                btn.classList.add('is-active');
            });
        });

        els.search()?.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') {
                e.preventDefault();
                if (typeof applyFilters === 'function') applyFilters();
            }
        });

        document.getElementById('btn-search-quick')?.addEventListener('click', () => {
            if (typeof applyFilters === 'function') applyFilters();
        });

        const applyBtn = els.applyBtn();
        if (applyBtn) {
            applyBtn.addEventListener('click', () => {
                if (typeof applyFilters === 'function') applyFilters();
            });
        }

        document.getElementById('btn-reset-filters')?.addEventListener('click', () => {
            if (typeof resetFilters === 'function') resetFilters();
        });

        const origApply = window.applyFilters;
        if (typeof origApply === 'function') {
            window.applyFilters = async function () {
                syncUi(false);
                const btn = els.applyBtn();
                btn?.classList.add('is-loading');
                try {
                    await origApply.apply(this, arguments);
                    updateChips();
                } finally {
                    btn?.classList.remove('is-loading');
                }
            };
        }

        const origReset = window.resetFilters;
        if (typeof origReset === 'function') {
            window.resetFilters = function () {
                origReset.apply(this, arguments);
                resetUi();
            };
        }
    }

    function updateResultHint(total) {
        const hint = document.getElementById('filter-result-hint');
        if (!hint) return;
        if (typeof total === 'number' && total >= 0) {
            hint.innerHTML = 'Tìm thấy <strong>' + total.toLocaleString('vi-VN') + '</strong> sản phẩm';
        } else {
            hint.textContent = 'Nhập từ khóa hoặc chọn bộ lọc';
        }
    }

    window.ProductFilter = { resetUi, updateChips, syncUi, updateResultHint };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
