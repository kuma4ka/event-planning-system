/**
 * Help Center Search Logic
 * Filters Topic cards and FAQ items based on user input.
 */

export function initHelpCenter() {
    const searchInput = document.getElementById('helpSearchInput');
    const topicCards = document.querySelectorAll('.support-topic-card'); // Parent is col-md-4
    const faqItems = document.querySelectorAll('.accordion-item');


    if (!searchInput) return;

    searchInput.addEventListener('input', (e) => {
        const query = e.target.value.toLowerCase().trim();
        let hasVisibleContent = false;

        // 1. Filter Topic Cards
        topicCards.forEach(card => {
            const title = card.querySelector('.support-topic-title')?.textContent.toLowerCase() || '';
            const desc = card.querySelector('.support-topic-desc')?.textContent.toLowerCase() || '';
            const parentCol = card.closest('.col-md-4');

            if (title.includes(query) || desc.includes(query)) {
                parentCol.style.display = 'block';
                // Add fade-in animation reset if needed, or just show
                card.classList.remove('d-none');
                hasVisibleContent = true;
            } else {
                parentCol.style.display = 'none';
            }
        });

        // 2. Filter FAQ Items
        faqItems.forEach(item => {
            const question = item.querySelector('.accordion-button')?.textContent.toLowerCase() || '';
            const answer = item.querySelector('.accordion-body')?.textContent.toLowerCase() || '';

            if (question.includes(query) || answer.includes(query)) {
                item.style.display = 'block';
                hasVisibleContent = true;

                // Optional: Expand item if query matches answer but not question? 
                // For now, let's keep it simple.
            } else {
                item.style.display = 'none';
            }
        });

        // 4. Show No Results Message
        const existingMsg = document.getElementById('noResultsMsg');

        if (!hasVisibleContent) {
            if (!existingMsg) {
                createNoResultsMessage();
            } else {
                existingMsg.style.display = 'block';
            }
        } else {
            if (existingMsg) existingMsg.style.display = 'none';
        }
    });

    function createNoResultsMessage() {
        const emptyState = document.createElement('div');
        emptyState.id = 'noResultsMsg';
        emptyState.className = 'text-center py-5 animate-fade-in';
        emptyState.innerHTML = `
            <div class="mb-3 text-muted opacity-50">
                <i class="bi bi-search fs-1"></i>
            </div>
            <h4 class="fw-bold text-muted">No results found</h4>
            <p class="text-muted small">Try adjusting your search terms</p>
        `;

        // Append after the last container content
        const container = document.querySelector('.container.py-5');
        if (container) container.appendChild(emptyState);
    }
}
