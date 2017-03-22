/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";

    angular
        .module("app")
        .filter("yesNo", function () { return function (text) { return text ? "Yes" : "No"; } });

})();
