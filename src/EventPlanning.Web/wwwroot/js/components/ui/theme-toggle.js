export function initThemeToggle() {
    const toggleBtn = document.getElementById('themeToggle');
    const icon = toggleBtn?.querySelector('i');
    const themeAttr = 'data-theme';
    const storageKey = 'theme-preference';

    const savedTheme = localStorage.getItem(storageKey);
    const systemPrefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;

    let currentTheme = savedTheme || (systemPrefersDark ? 'dark' : 'light');

    document.documentElement.setAttribute(themeAttr, currentTheme);
    updateIcon(currentTheme);

    if (!toggleBtn) return;

    toggleBtn.addEventListener('click', () => {
        currentTheme = currentTheme === 'light' ? 'dark' : 'light';
        document.documentElement.setAttribute(themeAttr, currentTheme);
        localStorage.setItem(storageKey, currentTheme);
        updateIcon(currentTheme);
    });

    function updateIcon(theme) {
        if (!icon) return;
        if (theme === 'dark') {
            icon.classList.remove('bi-moon', 'bi-moon-fill');
            icon.classList.add('bi-sun-fill');
            toggleBtn.setAttribute('title', 'Switch to Light Mode');
        } else {
            icon.classList.remove('bi-sun', 'bi-sun-fill');
            icon.classList.add('bi-moon-fill');
            toggleBtn.setAttribute('title', 'Switch to Dark Mode');
        }
    }
}
