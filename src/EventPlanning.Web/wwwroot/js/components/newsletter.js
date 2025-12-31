/**
 * Newsletter Subscription Component
 * Handles the footer newsletter subscription form submission.
 */
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

        // Disable button to prevent double submit
        this.setLoading(true);

        try {
            const formData = new FormData(this.form);

            const response = await fetch('/newsletter/subscribe', {
                method: 'POST',
                body: formData
            });

            const result = await response.json();

            if (result.success) {
                this.showToast('Success', result.message, 'success');
                this.form.reset();
            } else {
                this.showToast('Error', result.message, 'error');
            }
        } catch (error) {
            console.error('Newsletter error:', error);
            this.showToast('Error', 'Something went wrong. Please try again.', 'error');
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

    showToast(title, message, type) {
        if (window.showToast) {
            window.showToast(message, type);
        } else {
            alert(`${title}: ${message}`);
        }
    }
}
