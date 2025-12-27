import { initDashboard } from './components/dashboard.js';
import { initVenueManagement } from './components/admin/venues.js';
import { initEventDetails } from './components/event-details.js';

document.addEventListener('DOMContentLoaded', () => {

    initDashboard();
    initVenueManagement();
    initEventDetails();

    console.log('Stanza JS loaded modules successfully.');
});