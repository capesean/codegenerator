/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";

    angular
        .module("app")
        .directive("appPagerInfo", appPagerInfo);

    function appPagerInfo() {

        return {
            template: "<div>{{info}}</div>",
            restrict: "E",
            replace: true,
            link: function (scope, element, attrs, ctrl) {
                var c = ctrl;
                scope.$watch("headers", function (headers) {
                    if (headers) {
                        if (headers.totalRecords === 0)
                            scope.info = "No records found";
                        else if (headers.totalRecords === 1)
                            scope.info = "Showing the only record";
                        else if (headers.totalPages === 1 || headers.pageSize === 0)
                            scope.info = "Showing all " + headers.totalRecords + " records";
                        else if (headers.pageIndex === headers.totalPages - 1)
                            scope.info = "Showing " + (headers.pageIndex * headers.pageSize + 1) + " to " + headers.totalRecords + " of " + headers.totalRecords + " records";
                        else
                            scope.info = "Showing " + (headers.pageIndex * headers.pageSize + 1) + " to " + ((headers.pageIndex + 1) * headers.pageSize) + " of " + headers.totalRecords + " records";
                    }
                });
            },
            scope: {
                headers: "="
            }
        }
    }


})();