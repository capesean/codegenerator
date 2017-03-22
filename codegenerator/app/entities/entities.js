/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";
    angular
        .module("app")
        .controller("entities", entities);
    entities.$inject = ["$scope", "$state", "$q", "$timeout", "notifications", "appSettings", "entityResource"];
    function entities($scope, $state, $q, $timeout, notifications, appSettings, entityResource) {
        var vm = this;
        vm.loading = true;
        vm.appSettings = appSettings;
        vm.search = {};
        vm.runSearch = runSearch;
        vm.goToEntity = function (projectId, entityId) { return $state.go("app.entity", { projectId: projectId, entityId: entityId }); };
        vm.moment = moment;
        initPage();
        function initPage() {
            var promises = [];
            $q.all(promises).finally(function () { return runSearch(0); });
        }
        function runSearch(pageIndex) {
            vm.loading = true;
            var promises = [];
            promises.push(entityResource.query({
                q: vm.search.q,
                pageIndex: pageIndex
            }, function (data, headers) {
                vm.entities = data;
                vm.headers = JSON.parse(headers("X-Pagination"));
            }, function (err) {
                notifications.error("Failed to load the entities.", "Error", err);
                $state.go("app.home");
            }).$promise);
            $q.all(promises).finally(function () { return vm.loading = false; });
        }
        ;
    }
    ;
}());
//# sourceMappingURL=entities.js.map