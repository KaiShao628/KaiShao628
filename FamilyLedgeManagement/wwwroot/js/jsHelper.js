window.resizeTableByClassByTimes = function (times) {
    if (times === null || times === undefined) {
        times = 1;
    }

    if (times <= 0) {
        return;
    }

    window.resizeTableByClass();

    setTimeout(function () {
        window.resizeTableByClassByTimes(times - 1);
    }, 100);
};

window.resizeTableByClass = function () {
    if (typeof window.$ !== "function") {
        return false;
    }

    var tables = window.$('.table-resizable-div');
    if (tables !== null && tables !== undefined && tables.length > 0) {
        var miss = false;
        tables.each(function (index, element) {
            var searchbar = element.querySelector('.table-resizable-searchbar');
            var table = element.querySelector('.table-resizable-table');
            if (table !== null && table !== undefined) {
                var result = window.resizeOrderTableInner(table, searchbar);
                if (!result) {
                    miss = true;
                }
            }
        });
        return !miss;
    }

    return false;
};

window.resizeOrderTableInner = function (table, searchBar) {
    if (table === undefined || table === null) {
        return false;
    }

    var barHeight = 0;
    if (searchBar !== undefined && searchBar !== null) {
        barHeight = searchBar.offsetHeight;
    }

    table.style.setProperty('height', 'calc(100% - ' + barHeight + 'px)');

    var tableContainer = table.querySelector('.table-container');
    if (tableContainer === undefined || tableContainer === null) {
        return false;
    }

    var pagination = tableContainer.querySelector('.nav.nav-pages');
    var tableWapper = tableContainer.querySelector('.table-fixed');
    var toolbar = tableContainer.querySelector('.table-toolbar');
    var paginationHeight = 0;

    if (tableWapper === undefined || tableWapper === null) {
        return false;
    }

    if (pagination !== undefined && pagination !== null) {
        paginationHeight += (pagination.offsetHeight + 8);
    }

    if (toolbar !== undefined && toolbar !== null) {
        paginationHeight += toolbar.offsetHeight;
    }

    var innerSearchBar = tableContainer.querySelector('.table-search');
    if (innerSearchBar !== undefined && innerSearchBar !== null) {
        paginationHeight += innerSearchBar.offsetHeight;
    }

    tableWapper.style.setProperty('height', 'calc(100% - ' + paginationHeight + 'px)');

    var tableHeader = tableWapper.querySelector('.table-fixed-header');
    if (tableHeader !== undefined && tableHeader !== null) {
        var headerHeight = tableHeader.offsetHeight;
        var tableBody = tableWapper.querySelector('.table-fixed-body');
        if (tableBody !== undefined && tableBody !== null) {
            tableBody.style.setProperty('height', 'calc(100% - ' + headerHeight + 'px)');
        }
    }

    return true;
};
window.RemovePopover = function () {
    $(".popover.select.shadow.bs-popover-auto.fade.popover-dropdown.show").remove();
}
