(function () {
    const searchInput = document.getElementById('global-search');

    if (!searchInput) {
        return;
    }

    const filterRows = (query) => {
        const normalized = query.trim().toLowerCase();
        const rows = document.querySelectorAll('tr[data-search-row]');

        rows.forEach((row) => {
            const rowText = row.textContent?.toLowerCase() ?? '';
            row.style.display = normalized === '' || rowText.includes(normalized) ? '' : 'none';
        });
    };

    searchInput.addEventListener('input', (event) => {
        const query = event.target.value;
        filterRows(query);
    });
})();
