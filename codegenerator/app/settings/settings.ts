/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";

    angular
        .module("app")
        .controller("settings", settings);

    settings.$inject = ["$scope", "$state", "$stateParams", "settingsResource", "notifications", "appSettings", "errorService"];

    function settings($scope, $state, $stateParams, settingsResource, notifications, appSettings, errorService) {

        var vm = this;
        vm.loading = true;
        vm.settings = new settingsResource();
        vm.save = saveSettings;

		initPage();

        // events
		function initPage() {
            
            //vm.settings.xxx = appSettings.xxx;

            vm.loading = false;
		}

        function saveSettings() {

			if ($scope.mainForm.$invalid) {

				notifications.error("The form has not been completed correctly.", "Error");

			} else {

				vm.loading = true;
				vm.settings.$save(
					data => {

						vm.client = data;
                        notifications.success("The settings has been saved.", "Saved");

						// update the global appSettings variable
                        //appSettings.xxx = vm.settings.xxx;

					},
					err => {

						errorService.handleApiError(err, "settings");

					})
					.finally(() => vm.loading = false);

			}
        };

    };

} ());