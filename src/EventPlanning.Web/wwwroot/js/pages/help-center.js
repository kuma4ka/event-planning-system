export function initHelpCenter() {
    const searchInput = document.getElementById('helpSearchInput');
    const topicCards = document.querySelectorAll('.support-topic-card'); // Parent is col-md-4
    const faqItems = document.querySelectorAll('.accordion-item');

    if (!searchInput) return;

    searchInput.addEventListener('input', (e) => {
        const query = e.target.value.toLowerCase().trim();
        let hasVisibleContent = false;

        topicCards.forEach(card => {
            const title = card.querySelector('.support-topic-title')?.textContent.toLowerCase() || '';
            const desc = card.querySelector('.support-topic-desc')?.textContent.toLowerCase() || '';
            const parentCol = card.closest('.col-md-4');

            if (title.includes(query) || desc.includes(query)) {
                parentCol.style.display = 'block';
                card.classList.remove('d-none');
                hasVisibleContent = true;
            } else {
                parentCol.style.display = 'none';
            }
        });

        faqItems.forEach(item => {
            const question = item.querySelector('.accordion-button')?.textContent.toLowerCase() || '';
            const answer = item.querySelector('.accordion-body')?.textContent.toLowerCase() || '';

            if (question.includes(query) || answer.includes(query)) {
                item.style.display = 'block';
                hasVisibleContent = true;

            } else {
                item.style.display = 'none';
            }
        });

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

        const container = document.querySelector('.container.py-5');
        if (container) container.appendChild(emptyState);
    }
}
