/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";
    angular
        .module("app")
        .controller("home", home);
    home.$inject = ["authService", "$rootScope", "appSettings", "$scope", "$state", "$timeout", "$resource", "projectResource", "notifications", "$stateParams"];
    function home(authService, $rootScope, appSettings, $scope, $state, $timeout, $resource, projectResource, notifications, $stateParams) {
        var vm = this;
        vm.appSettings = appSettings; //todo: remove
        vm.goToProject = function (projectId) { return $state.go("app.project", { projectId: projectId }); };
        initPage();
        function initPage() {
            vm.identity = $rootScope.identity;
            projectResource.query({
                pageSize: 0
            }, function (data) {
                vm.projects = data;
            }, function (err) {
                notifications.error("Failed to load the projects.", "Error", err);
            }).$promise.then(function () { return vm.loading = false; });
        }
    }
}());
//# sourceMappingURL=home.js.map