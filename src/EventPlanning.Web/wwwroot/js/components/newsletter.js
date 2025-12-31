export class NewsletterSubscription {
    constructor() {
        this.form = document.getElementById('newsletter-form');
        this.input = document.getElementById('newsletter-email');
        this.button = document.getElementById('newsletter-submit');

        if (this.form) {
            this.init();
        }
    }

    init() {
        this.form.addEventListener('submit', (e) => this.handleSubmit(e));
    }

    async handleSubmit(e) {
        e.preventDefault();

        const email = this.input.value;
        if (!email) return;

        this.setLoading(true);

        try {
            const formData = new FormData(this.form);

            const response = await fetch('/newsletter/subscribe', {
                method: 'POST',
                body: formData
            });

            const result = await response.json();

            if (result.success) {
                this.showModal('newsletterSuccessModal', result.message);
                this.form.reset();
            } else {
                this.showModal('newsletterErrorModal', result.message);
            }
        } catch (error) {
            console.error('Newsletter error:', error);
            this.showModal('newsletterErrorModal', 'Something went wrong. Please try again.');
        } finally {
            this.setLoading(false);
        }
    }

    setLoading(isLoading) {
        if (isLoading) {
            this.button.disabled = true;
            this.originalBtnText = this.button.innerHTML;
            this.button.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>';
        } else {
            this.button.disabled = false;
            this.button.innerHTML = this.originalBtnText || 'Subscribe';
        }
    }

    showModal(modalId, message) {
        if (message) {
            const messageEl = document.getElementById(
                modalId === 'newsletterSuccessModal' ? 'newsletterSuccessMessage' : 'newsletterErrorMessage'
            );
            if (messageEl) messageEl.textContent = message;
        }

        const modalEl = document.getElementById(modalId);
        if (modalEl && window.bootstrap) {
            const modal = new bootstrap.Modal(modalEl);
            modal.show();
        } else {
            alert(message);
        }
    }
}
