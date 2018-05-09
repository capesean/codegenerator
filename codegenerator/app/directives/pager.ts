/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";

    angular
        .module("app")
        .directive("appPager", appPager);

    function appPager() {

        return {
            templateUrl: "/app/directives/pager.html",
            restrict: "E",
            controller: appPagerController,
            replace: true,
            scope: {
                headers: "=",
                callback: "="
            }
        }
    }

    appPagerController.$inject = ["$scope"];
    function appPagerController($scope) {
        $scope.runSearch = function (pageIndex) {
            if ($scope.headers.pageIndex === pageIndex) return;
            $scope.callback(pageIndex);
        }
    }

})();