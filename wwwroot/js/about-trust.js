(function () {
    const section = document.querySelector('.about-trust');
    if (!section) return;

    const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    function animateValue(el) {
        if (el.dataset.animated === '1') return;
        el.dataset.animated = '1';

        const target = parseInt(el.dataset.count, 10) || 0;
        const suffix = el.dataset.suffix || '';
        const prefix = el.dataset.prefix || '';
        const duration = reducedMotion ? 0 : 1400;
        const start = performance.now();

        function tick(now) {
            const t = duration === 0 ? 1 : Math.min((now - start) / duration, 1);
            const eased = 1 - Math.pow(1 - t, 3);
            const current = Math.round(target * eased);
            el.textContent = prefix + current.toLocaleString('vi-VN') + suffix;
            if (t < 1) requestAnimationFrame(tick);
        }

        requestAnimationFrame(tick);
    }

    const revealObserver = new IntersectionObserver(
        (entries) => {
            entries.forEach((entry) => {
                if (!entry.isIntersecting) return;
                entry.target.classList.add('is-visible');
                const counter = entry.target.querySelector('[data-count]');
                if (counter) animateValue(counter);
                revealObserver.unobserve(entry.target);
            });
        },
        { threshold: 0.12, rootMargin: '0px 0px -30px 0px' }
    );

    section.querySelectorAll('.reveal').forEach((el) => revealObserver.observe(el));

    if (reducedMotion) {
        section.querySelectorAll('[data-count]').forEach((el) => {
            const target = parseInt(el.dataset.count, 10) || 0;
            const suffix = el.dataset.suffix || '';
            const prefix = el.dataset.prefix || '';
            el.textContent = prefix + target.toLocaleString('vi-VN') + suffix;
        });
        section.querySelectorAll('.reveal').forEach((el) => el.classList.add('is-visible'));
    }
})();
