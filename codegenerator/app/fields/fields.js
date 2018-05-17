/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";
    angular
        .module("app")
        .controller("fields", fields);
    fields.$inject = ["$scope", "$state", "$q", "$timeout", "notifications", "appSettings", "fieldResource"];
    function fields($scope, $state, $q, $timeout, notifications, appSettings, fieldResource) {
        var vm = this;
        vm.loading = true;
        vm.appSettings = appSettings;
        vm.search = {};
        vm.searchObjects = {};
        vm.runSearch = runSearch;
        vm.goToField = function (projectId, entityId, fieldId) { return $state.go("app.field", { projectId: projectId, entityId: entityId, fieldId: fieldId }); };
        vm.moment = moment;
        initPage();
        function initPage() {
            var promises = [];
            promises.push(runSearch(0, true));
            $q.all(promises).finally(function () { return vm.loading = false; });
        }
        function runSearch(pageIndex, dontSetLoading) {
            if (!dontSetLoading)
                vm.loading = true;
            vm.search.includeEntities = true;
            vm.search.pageIndex = pageIndex;
            var promise = fieldResource.query(vm.search, function (data, headers) {
                vm.fields = data;
                vm.headers = JSON.parse(headers("X-Pagination"));
            }, function (err) {
                notifications.error("Failed to load the fields.", "Error", err);
                $state.go("app.home");
            }).$promise;
            if (!dontSetLoading)
                promise.then(function () { return vm.loading = false; });
            return promise;
        }
        ;
    }
    ;
}());
//# sourceMappingURL=fields.js.map