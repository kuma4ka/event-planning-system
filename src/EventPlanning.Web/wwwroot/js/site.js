import { initDashboard } from './components/dashboard.js';
import { initVenueManagement } from './components/admin/venues.js';
import { initEventDetails } from './components/event/event-details.js';
import { initEventActions } from "./components/event/event-actions.js";
import { initViewToggle } from "./components/ui/view-toggle.js";
import { initThemeToggle } from "./components/ui/theme-toggle.js";
import { initHelpCenter } from "./pages/help-center.js";

document.addEventListener('DOMContentLoaded', () => {
    initThemeToggle();


    initDashboard();
    initVenueManagement();
    initEventDetails();
    initEventActions();
    initViewToggle();
    initHelpCenter();

    console.log('Stanza JS loaded modules successfully.');
});