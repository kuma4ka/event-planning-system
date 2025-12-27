import { initDashboard } from './components/dashboard.js';
import { initVenueManagement } from './components/admin/venues.js';

document.addEventListener('DOMContentLoaded', () => {

    initDashboard();

    initVenueManagement();

    console.log('Stanza JS loaded modules successfully.');
});