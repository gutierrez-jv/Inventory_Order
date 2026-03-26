/*
    site.js - Purpose

    This file handles small frontend interactions to improve user experience.

    It is responsible for:
    - Showing popup messages (success, error, warning, info) using SweetAlert
    - Displaying validation errors in a cleaner way
    - Reading messages from the server (e.g., success/error after actions)
    - Filtering/searching table data dynamically

    This file does NOT handle core system logic.
    The main logic of the system is handled in:
    - Controllers (request handling)
    - Services (business rules)
    - Repositories (database access)

    site.js is only for client-side behavior and UI enhancements.
*/

// This whole file is likely wrapped inside an IIFE:
// (() => { ... })();
// That means it runs immediately once the script loads,
// and keeps variables/functions private so they do not pollute global scope.


// Checks if SweetAlert is loaded in the page.
// window.Swal refers to the SweetAlert library object.
// typeof ... === 'undefined' means the library is missing.
if (typeof window.Swal === 'undefined') {
    // Stop running the script if SweetAlert does not exist.
    // This prevents JavaScript errors like "Swal is not defined".
    return;
}


// Reusable helper function for showing SweetAlert popups.
// icon  = success / error / warning / info / question
// title = popup title
// html  = popup content/body
const showSwal = (icon, title, html) =>
    Swal.fire({ icon, title, html, confirmButtonText: 'OK' });


// Converts text into safe HTML.
// This is used to prevent HTML/script injection (XSS).
// Example:
// if a user enters <script>alert(1)</script>,
// this function makes it display as plain text instead of executing.
const escapeHtml = (value) => {
    const div = document.createElement('div'); // creates a temporary <div>
    div.textContent = value;                   // stores the value as plain text
    return div.innerHTML;                     // returns the safe HTML-escaped version
};


// Collects text from a list of elements and removes duplicates.
// Used for gathering validation error messages from the page.
const collectErrors = (elements) => {
    const result = []; // temporary array to store messages

    elements.forEach((el) => {
        const text = (el.textContent || '').trim(); // get text and remove extra spaces
        if (text) {
            result.push(text); // only add non-empty messages
        }
    });

    // new Set(result) removes duplicate messages
    // [...new Set(result)] converts it back into an array
    return [...new Set(result)];
};


// Displays a list of errors inside a SweetAlert popup.
function showErrors(title, errors) {
    // If there are no errors, do nothing.
    if (!errors.length) {
        return;
    }

    // Convert each error into an <li> item
    // escapeHtml(err) is used for safety
    const list = errors.map((err) => `<li>${escapeHtml(err)}</li>`).join('');

    // Show the list inside a popup
    showSwal('error', title, `<ul style="text-align:left;">${list}</ul>`);
}


// Reads server-side messages from a hidden HTML element
// and shows them in a SweetAlert popup.
//
// Usually used with TempData / ViewData from ASP.NET MVC,
// where the server sends a message like success or error.
function showServerMessage() {
    // Looks for an element with this ID in the page
    const source = document.getElementById('swal-server-messages');

    // If that element does not exist, stop
    if (!source) return;

    // Reads possible message types from data-* attributes
    // Example:
    // data-success="Order created successfully"
    // data-error="Something went wrong"
    const options = [
        ['success', 'Success', source.dataset.success],
        ['error', 'Error', source.dataset.error],
        ['warning', 'Warning', source.dataset.warning],
        ['info', 'Notice', source.dataset.info]
    ];

    // Find the first non-empty message among success/error/warning/info
    const active = options.find(([, , text]) => text && text.trim());

    // If no message exists, stop
    if (!active) return;

    // Show the popup using the message type found
    showSwal(active[0], active[1], `<p>${escapeHtml(active[2])}</p>`);
}


// Looks for ASP.NET validation summary errors already rendered in the page
// and shows them as a SweetAlert popup.
function showModelSummaryErrors() {
    // Selects all <li> validation summary items
    const summaryItems = document.querySelectorAll(
        '.validation-summary-errors li, [data-valmsg-summary="true"] li'
    );

    // Collects the messages, removes duplicates, then shows them
    showErrors('Please fix the following errors', collectErrors(summaryItems));
}


// Connects jQuery client-side validation to SweetAlert popups.
// This means instead of only inline validation text,
// it can show a popup when the form is invalid.
function wireClientValidationPopup() {
    // If jQuery is not loaded, stop
    if (typeof window.jQuery === 'undefined') return;

    // Select all forms in the page
    const forms = document.querySelectorAll('form');

    forms.forEach((form) => {
        const $form = window.jQuery(form); // wraps the form as a jQuery object

        // If this form has no jQuery validator attached, skip it
        if (!$form.data('validator')) return;

        // Listen when the form is submitted
        $form.on('submit', function (event) {
            // If form is valid, allow normal submit
            if ($form.valid()) return;

            // If form is invalid, stop submission
            event.preventDefault();

            // Get validation messages from jQuery validate
            const errors = $form.validate().errorList.map((x) => x.message);

            // Show errors in popup, removing duplicates
            showErrors('Please complete the required fields', [...new Set(errors)]);
        });
    });
}


// Get the global search textbox
const searchInput = document.getElementById('global-search');

// Get the dropdown/select for choosing which field to search
const searchField = document.getElementById('global-search-field');


// Filters table rows based on the search input.
// This is usually used in tables like Orders, Products, Customers.
function filterRows() {
    // Read the typed search text and convert it to lowercase
    const query = (searchInput.value || '').trim().toLowerCase();

    // Read which field to search by.
    // If no dropdown exists, default to searching all text.
    const field = searchField ? searchField.value : 'all';

    // Select all table rows marked as searchable
    const rows = document.querySelectorAll('tr[data-search-row]');

    rows.forEach((row) => {
        let text = '';

        if (field === 'all') {
            // Search the whole row text
            text = row.textContent || '';
        } else {
            // Search a specific custom attribute first, fallback to full row text
            // Example: data-customer="vince"
            text = row.getAttribute('data-' + field) || row.textContent || '';
        }

        text = text.toLowerCase();

        // Show row if:
        // - query is empty, or
        // - row text contains the query
        // Otherwise, hide the row
        row.style.display = query === '' || text.includes(query) ? '' : 'none';
    });
}


// If the search input exists,
// run filtering every time the user types something
if (searchInput) {
    searchInput.addEventListener('input', filterRows);
}


// If the search field dropdown exists,
// run filtering every time the chosen field changes
if (searchField) {
    searchField.addEventListener('change', filterRows);
}


// Run these functions immediately when the file loads:

showServerMessage();         // shows server-side success/error message popup
showModelSummaryErrors();   // shows ASP.NET validation summary errors in popup
wireClientValidationPopup(); // hooks client-side validation into popup alerts

// Then the IIFE likely ends with:
}) ();