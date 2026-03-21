(function () {
    const searchInput = document.getElementById('global-search');
    const searchField = document.getElementById('global-search-field');

    if (!searchInput) {
        return;
    }

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

    searchInput.addEventListener('input', filterRows);

    if (searchField) {
        searchField.addEventListener('change', filterRows);
    }
})();
