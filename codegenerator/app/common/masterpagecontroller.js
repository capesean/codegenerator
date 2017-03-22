/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";
    angular
        .module("app")
        .controller("masterPageController", masterPageController);
    masterPageController.$inject = ["$rootScope", "$window", "localStorageService", "appSettings", "authService"];
    function masterPageController($rootScope, $window, localStorageService, appSettings, authService) {
        var vm = this;
        vm.identity = null;
        vm.logout = logout;
        vm.minimizedMenu = false;
        vm.minimizeMenu = minimizeMenu;
        vm.maximizeMenu = maximizeMenu;
        vm.appSettings = appSettings;
        initPage();
        function initPage() {
            if (localStorageService.get("minimizeMenu") === 1)
                vm.minimizedMenu = true;
            $rootScope.$watch("isLoaded", function () {
                vm.identity = $rootScope.identity;
                vm.hideLeftMenu = $rootScope.hideLeftMenu;
                vm.isAdmin = $rootScope.isAdmin;
                vm.isReports = $rootScope.isReports;
            });
        }
        function logout() {
            $window.location.assign("/logout");
        }
        function minimizeMenu() {
            localStorageService.set("minimizeMenu", 1);
            vm.minimizedMenu = true;
        }
        function maximizeMenu() {
            localStorageService.set("minimizeMenu", 0);
            vm.minimizedMenu = false;
        }
    }
})();
//# sourceMappingURL=masterpagecontroller.js.map