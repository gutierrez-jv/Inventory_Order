(function () {
    const hasSwal = typeof window.Swal !== 'undefined';
    // Checks if SweetAlert is loaded and available on the page

    const showSwal = (icon, title, html) => {
        if (!hasSwal) return;
        // Prevents execution if SweetAlert is not available

        window.Swal.fire({ icon, title, html, confirmButtonText: 'OK' });
        // Displays a SweetAlert popup with the given icon, title, and HTML content
    };

    const escapeHtml = (value) => {
        const div = document.createElement('div');
        // Creates a temporary div element for safe text conversion

        div.textContent = value;
        // Stores the value as plain text to avoid injecting unsafe HTML

        return div.innerHTML;
        // Returns the escaped HTML version of the text
    };

    const collectErrors = (elements) => {
        const result = [];
        // Stores all extracted validation error messages

        elements.forEach((el) => {
            const text = (el.textContent || '').trim();
            // Gets and trims the text content of each validation element

            if (text) result.push(text);
            // Adds the error message if it is not empty
        });

        return [...new Set(result)];
        // Removes duplicate error messages before returning them
    };

    function showErrors(title, errors) {
        if (!errors.length) return;
        // Stops if there are no errors to display

        const list = errors.map((err) => `<li>${escapeHtml(err)}</li>`).join('');
        // Converts each error into a safe HTML list item

        showSwal('error', title, `<ul style="text-align:left;">${list}</ul>`);
        // Shows all collected errors inside a SweetAlert popup
    }

    function showServerMessage() {
        if (!hasSwal) return;
        // Stops if SweetAlert is not available

        const source = document.getElementById('swal-server-messages');
        // Gets the hidden element that stores server-side TempData messages

        if (!source) return;
        // Stops if the message source element does not exist

        const options = [
            ['success', 'Success', source.dataset.success],
            ['error', 'Error', source.dataset.error],
            ['warning', 'Warning', source.dataset.warning],
            ['info', 'Notice', source.dataset.info]
        ];
        // Stores the possible server message types and their display titles

        const active = options.find(([, , text]) => text && text.trim());
        // Finds the first message type that contains a value

        if (!active) return;
        // Stops if there is no active server message to display

        showSwal(active[0], active[1], `<p>${escapeHtml(active[2])}</p>`);
        // Displays the active server message in a SweetAlert popup
    }

    function showModelSummaryErrors() {
        if (!hasSwal) return;
        // Stops if SweetAlert is not available

        const summaryItems = document.querySelectorAll('.validation-summary-errors li, [data-valmsg-summary="true"] li');
        // Selects all validation summary error list items from the page

        showErrors('Please fix the following errors', collectErrors(summaryItems));
        // Collects and displays the validation summary errors
    }

    function wireClientValidationPopup() {
        if (!hasSwal) return;
        // Stops if SweetAlert is not available

        if (typeof window.jQuery === 'undefined') return;
        // Stops if jQuery is not loaded

        const forms = document.querySelectorAll('form');
        // Selects all forms on the page

        forms.forEach((form) => {
            const $form = window.jQuery(form);
            // Wraps the current form as a jQuery object

            if (!$form.data('validator')) return;
            // Skips forms that do not use jQuery validation

            $form.on('submit', function (event) {
                if ($form.valid()) return;
                // Allows submission if the form passes validation

                event.preventDefault();
                // Prevents form submission if validation fails

                const errors = $form.validate().errorList.map((x) => x.message);
                // Extracts all client-side validation error messages

                showErrors('Please complete the required fields', [...new Set(errors)]);
                // Displays the unique client-side validation errors in a popup
            });
        });
    }

    const searchInput = document.getElementById('global-search');
    // Gets the global search input field

    const searchField = document.getElementById('global-search-field');
    // Gets the dropdown used to choose which field to search

    function filterRows() {
        if (!searchInput) return;
        // Stops if the search input does not exist

        const query = (searchInput.value || '').trim().toLowerCase();
        // Gets and normalizes the search text

        const field = searchField ? searchField.value : 'all';
        // Gets the selected field, or defaults to searching all content

        const rows = document.querySelectorAll('tr[data-search-row]');
        // Selects all table rows marked as searchable

        rows.forEach((row) => {
            let text = '';
            // Holds the text that will be matched against the search query

            if (field === 'all') {
                text = row.textContent || '';
                // Uses the full row text when searching across all fields
            } else {
                text = row.getAttribute('data-' + field) || row.textContent || '';
                // Uses the row's specific data attribute when searching by a selected field
            }

            text = text.toLowerCase();
            // Converts the text to lowercase for case-insensitive searching

            row.style.display = query === '' || text.includes(query) ? '' : 'none';
            // Shows rows that match the query and hides those that do not
        });
    }

    if (searchInput) {
        searchInput.addEventListener('input', filterRows);
        // Filters table rows whenever the user types in the search box
    }

    if (searchField) {
        searchField.addEventListener('change', filterRows);
        // Filters table rows whenever the selected search field changes
    }

    showServerMessage();
    // Displays any server-side TempData message on page load

    showModelSummaryErrors();
    // Displays model validation summary errors on page load

    wireClientValidationPopup();
    // Enables SweetAlert popups for client-side validation errors
})();