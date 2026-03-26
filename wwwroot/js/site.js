(function () {
    if (typeof window.Swal === 'undefined') {
        return;
    }

    const showSwal = (icon, title, html) =>
        Swal.fire({ icon, title, html, confirmButtonText: 'OK' });

    const escapeHtml = (value) => {
        const div = document.createElement('div');
        div.textContent = value;
        return div.innerHTML;
    };

    const collectErrors = (elements) => {
        const result = [];
        elements.forEach((el) => {
            const text = (el.textContent || '').trim();
            if (text) {
                result.push(text);
            }
        });
        return [...new Set(result)];
    };

    function showErrors(title, errors) {
        if (!errors.length) {
            return;
        }

        const list = errors.map((err) => `<li>${escapeHtml(err)}</li>`).join('');
        showSwal('error', title, `<ul style="text-align:left;">${list}</ul>`);
    }

    function showServerMessage() {
        const source = document.getElementById('swal-server-messages');
        if (!source) return;

        const options = [
            ['success', 'Success', source.dataset.success],
            ['error', 'Error', source.dataset.error],
            ['warning', 'Warning', source.dataset.warning],
            ['info', 'Notice', source.dataset.info]
        ];

        const active = options.find(([, , text]) => text && text.trim());
        if (!active) return;

        showSwal(active[0], active[1], `<p>${escapeHtml(active[2])}</p>`);
    }

    function showModelSummaryErrors() {
        const summaryItems = document.querySelectorAll('.validation-summary-errors li, [data-valmsg-summary="true"] li');
        showErrors('Please fix the following errors', collectErrors(summaryItems));
    }

    function wireClientValidationPopup() {
        if (typeof window.jQuery === 'undefined') return;

        const forms = document.querySelectorAll('form');
        forms.forEach((form) => {
            const $form = window.jQuery(form);
            if (!$form.data('validator')) return;

            $form.on('submit', function (event) {
                if ($form.valid()) return;

                event.preventDefault();
                const errors = $form.validate().errorList.map((x) => x.message);
                showErrors('Please complete the required fields', [...new Set(errors)]);
            });
        });
    }

    const searchInput = document.getElementById('global-search');
    const searchField = document.getElementById('global-search-field');

    function filterRows() {
        const query = (searchInput.value || '').trim().toLowerCase();
        const field = searchField ? searchField.value : 'all';
        const rows = document.querySelectorAll('tr[data-search-row]');

        rows.forEach((row) => {
            let text = '';

            if (field === 'all') {
                text = row.textContent || '';
            } else {
                text = row.getAttribute('data-' + field) || row.textContent || '';
            }

            text = text.toLowerCase();
            row.style.display = query === '' || text.includes(query) ? '' : 'none';
        });
    }

    if (searchInput) {
        searchInput.addEventListener('input', filterRows);
    }

    if (searchField) {
        searchField.addEventListener('change', filterRows);
    }

    showServerMessage();
    showModelSummaryErrors();
    wireClientValidationPopup();
})();
