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
        vm.goToProject = projectId => $state.go("app.project", { projectId: projectId });

        initPage();

        function initPage() {
            vm.identity = $rootScope.identity;

            projectResource.query(
                {
                    pageSize: 0
                },
                data => {

                    vm.projects = data;

                },
                err => {

                    notifications.error("Failed to load the projects.", "Error", err);

                }).$promise.then(() => vm.loading = false);
        }
    }

} ());