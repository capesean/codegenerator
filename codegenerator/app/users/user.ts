/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";

    angular
        .module("app")
        .controller("user", user);

    user.$inject = ["$scope", "$state", "$stateParams", "userResource", "notifications", "appSettings", "$q", "errorService"];
    function user($scope, $state, $stateParams, userResource, notifications, appSettings, $q, errorService) {

        var vm = this;
        vm.loading = true;
        vm.appSettings = appSettings;
        vm.user = null;
        vm.save = saveUser;
        vm.isNew = $stateParams.id === appSettings.newGuid;

        initPage();

        function initPage() {
            var promises = [
            ];

            $q.all(promises)
                .then(() => {

                    if (vm.isNew) {

                        vm.user = new userResource();
                        vm.user.enabled = true;
                        vm.user.id = appSettings.newGuid;
                        vm.loading = false;

                    } else {

                        promises = [];

                        promises.push(
                            userResource.get(
                                {
                                    id: $stateParams.id
                                },
                                data => {
                                    vm.user = data;
                                },
                                err => {

                                    if (err.status === 404) {
                                        notifications.error("The requested user does not exist.", "Error");
                                    } else {
                                        notifications.error("Failed to load the user.", "Error", err);
                                    }

                                    $state.go("app.users");

                                })
                                .$promise);

                        $q.all(promises).finally(() => vm.loading = false);
                    }
                });
        }

        function saveUser() {

            if ($scope.mainForm.$invalid) {

                notifications.error("The form has not been completed correctly.", "Error");

            } else {

                vm.loading = true;

                vm.user.$save(
                    data => {

                        vm.user = data;
                        notifications.success("The user has been saved.", "Saved");
                        $state.go("app.user", { id: vm.user.id });

                    },
                    err => {

                        errorService.handleApiError(err, "user");

                    }).finally(() => vm.loading = false);

            }
        };

    };

}()); 