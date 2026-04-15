function resizeTableByClassByTimes(times) {
    if (times === null || times === undefined) {
        times = 1;
    }
    if (times > 0) {
        var result = resizeTableByClass();
        if (result) {
            //return;
        }
    }
    setTimeout(function () {
        resizeTableByClassByTimes(times - 1);
    }, 100);
}

function resizeTableByClass() {
    var tables = $('.table-resizable-div');
    if (tables !== null && tables !== undefined && tables.length > 0) {
        var miss = false;
        tables.each(function (index, element) {
            //找出搜索框
            var searchbar = element.querySelector('.table-resizable-searchbar');
            var table = element.querySelector('.table-resizable-table');
            if (table !== null && table !== undefined) {
                var result = resizeOrderTableInner(table, searchbar);
                if (!result) {
                    miss = true;
                }
            }
        });
        return !miss;
    }
    return false;
}

//重设表格高度以自适应 tableDivId 包裹table的div，searchBarDivId 包裹外置搜索框的div
function resizeOrderTableInner(table, searchBar) {

    //var table = $('#' + tableDivId);
    //document.querySelector('#oversea-order-table-div');
    if (table === undefined || table === null) {
        return false;
    }
    //var searchBar = document.querySelector('#' + searchBarDivId);
    var barHeight = 0;

    if (searchBar !== undefined && searchBar !== null) {
        //外置搜索栏高度
        barHeight = searchBar.offsetHeight;
        //table.css({ height: "calc(100% - " + barHeight + "px)" });
    }
    table.style.setProperty("height", "calc(100% - " + barHeight + "px)");

    var tableContainer = table.querySelector('.table-container');
    if (tableContainer === undefined || tableContainer === null) {
        return false;
    }
    //导航栏高度
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
        paginationHeight += (toolbar.offsetHeight);
    }
    //内置搜索框
    var innerSearchBar = tableContainer.querySelector('.table-search');
    if (innerSearchBar !== undefined && innerSearchBar !== null) {
        paginationHeight += innerSearchBar.offsetHeight;
    }
    //var tableWapper = table.find('.table-wrapper:first:first');
    if (tableWapper !== undefined && tableWapper !== null) {
        //设置表格wapper的高度
        //tableWapper.css({ height: "calc(100% - " + paginationHeight + "px)" });
        tableWapper.style.setProperty("height", "calc(100% - " + paginationHeight + "px)");
    }

    //表头
    var tableHeader = tableWapper.querySelector('.table-fixed-header');
    if (tableHeader !== undefined && tableHeader !== null) {
        var headerHeight = tableHeader.offsetHeight;
        var tableBody = tableWapper.querySelector('.table-fixed-body');
        //var tableBody = $('#oversea-order-table-div .table-fixed-body:first');
        if (tableBody !== undefined && tableBody !== null) {
            //设置表格body的高度
            //tableBody.css({ height: "calc(100% - " + headerHeight + "px)" });
            tableBody.style.setProperty("height", "calc(100% - " + headerHeight + "px)");
            //tableBody[0].style.height ="height:calc(100% - " + headerHeight + "px)";
        }
    }
    return true;
    //console.warn(barHeight); $('#oversea-order-table-div .table-fixed-body:first')  .css({ height: "calc(100% - " + 81 + "px)" });
}