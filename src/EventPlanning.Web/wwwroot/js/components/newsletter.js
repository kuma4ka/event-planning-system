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

            // Note: FormData automatically captures the inputs, including hidden anti-forgery token

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
        // Reuse existing toast system if available, or simple alert for now
        // Checking if we have a global toast function from site.js or UI library
        if (window.showToast) {
            window.showToast(message, type); // Assuming strict signature matches or similar
        } else {
            // Fallback to alert if no system toast
            alert(`${title}: ${message}`);
        }
    }
}
