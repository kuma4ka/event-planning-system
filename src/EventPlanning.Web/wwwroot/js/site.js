import { initDashboard } from './components/dashboard.js';
import { initVenueManagement } from './components/admin/venues.js';
import { initEventDetails } from './components/event/event-details.js';
import {initEventActions} from "./components/event/event-actions.js";
import { initViewToggle } from "./components/view-toggle.js";

document.addEventListener('DOMContentLoaded', () => {

    initDashboard();
    initVenueManagement();
    initEventDetails();
    initEventActions();
    initViewToggle();

    console.log('Stanza JS loaded modules successfully.');
});