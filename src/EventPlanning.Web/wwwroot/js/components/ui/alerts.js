export function initAlerts() {
    document.addEventListener('click', function (e) {
        const dismissBtn = e.target.closest('.stanza-alert-close');
        if (dismissBtn) {
            const alert = dismissBtn.closest('.stanza-alert');
            if (alert) {
                alert.style.transition = 'opacity 0.3s ease, transform 0.3s ease';
                alert.style.opacity = '0';
                alert.style.transform = 'translateX(20px)';

                setTimeout(() => {
                    alert.remove();
                }, 300);
            }
        }
    });
}
