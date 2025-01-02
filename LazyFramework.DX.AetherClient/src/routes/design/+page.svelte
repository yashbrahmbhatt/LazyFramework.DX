<script lang="ts" module>
</script>

<script lang="ts">
	import type { PageData } from './$types';
	import { ChevronDown, ChevronUp, Plus, X } from 'lucide-svelte';
	import Button from '$lib/components/ui/button/button.svelte';
	import ScrollArea from '$lib/components/ui/scroll-area/scroll-area.svelte';

	import * as Tabs from '$lib/components/ui/tabs';
	import MermaidViewer from '$lib/composites/mermaid-viewer.svelte';
	import DesignEditor from '$lib/components/ui/design-editor/design-editor.svelte';
	import {
		createSampleData,
		TriggerType,
		type SolutionData
	} from '$lib/components/ui/design-editor/models';
	import { writable } from 'svelte/store';
	import { createDesignEditorColumns } from '$lib/components/ui/design-editor/columns';
	let { data }: { data: PageData } = $props();
	let solution = writable(createSampleData());
	let cols = writable(createDesignEditorColumns(solution));
	let mermaidTemplate = `		graph TD
		%% Orchestrator subgraph
		subgraph OrchestratorNode[Orchestrator]
		    subgraph QueuesNode[Queues]
		        {{QueueNodeDefinitions}}
		    end
		    subgraph TasksNode[Tasks]
		       {{TaskNodeDefinitions}}
		    end
		    subgraph TriggersNode[Triggers]
		        {{TriggerNodeDefinitions}}
		    end
		end

		%% Robots subgraph
		subgraph RobotsNode[Robots]
		    {{RobotNodeDefinitions}}
		end

		%% Libraries subgraph
		subgraph LibrariesNode[Libraries]
		    {{LibraryNodeDefinitions}}
		end

		%% Target Applications subgraph
		subgraph TargetApplicationsNode[Target Applications]
		    {{TargetApplicationDefinitions}}
		end

		 %% Connections
		 {{Connections}}`;
	// `block-beta
	// %% Orchestrator block
	// columns 6
	// block:OrchestratorNode["Orchestrator"]:3
	//     block:QueuesNode["Queues"]:1
	// 		columns 1
	//     	{{QueueNodeDefinitions}}
	// 	end
	// 	block:TasksNode["Tasks"]:1
	// 		columns 1
	//    	{{TaskNodeDefinitions}}
	// 	end
	// 	block:TriggersNode["Triggers"]:1
	// 		columns 1
	//     	{{TriggerNodeDefinitions}}
	// 	end
	// end

	// %% Robots block
	// block:RobotsNode["Robots"]:1
	// 	columns 1
	// 	{{RobotNodeDefinitions}}
	// end

	// %% Libraries block
	// block:LibrariesNode["Libraries"]:1
	// 	columns 1
	// 	{{LibraryNodeDefinitions}}
	// end

	// %% Target Applications block
	// block:TargetApplicationsNode["Target Applications"]:1
	// 	columns 1
	// 	{{TargetApplicationDefinitions}}
	// end

	// %% Connections
	// {{Connections}}
	// `
	let mermaidStr = $derived(generateMermaid($solution));
	function generateMermaid(sol: SolutionData) {
		const { robots, applications, libraries, queues, tasks, triggers } = sol;
		const queueNodes = queues.map((queue) => `${queue.id}["${queue.name}"]`).join('\n');
		const taskNodes = tasks.map((task) => `${task.id}["${task.name}"]`).join('\n');
		const robotNodes = robots.map((robot) => `${robot.id}["${robot.name}"]`).join('\n');
		const libraryNodes = libraries.map((library) => `${library.id}["${library.name}"]`).join('\n');
		const targetApplicationNodes = applications.map((app) => `${app.id}["${app.name}"]`).join('\n');
		const triggerNodes = triggers.map((trigger) => `${trigger.id}["${trigger.name}"]`).join('\n');
		const libraryToApplicationConnections = libraries
			.filter((library) => library.application !== '')
			.map((library) => `${library.id} --> ${library.application}`)
			.join('\n');
		const queuesToTriggers = triggers
			.filter((trigger) => trigger.triggerType === TriggerType.Queue)
			.map((trigger) => `${trigger.queueId} -..-> ${trigger.id}`)
			.join('\n');
		const triggersToRobots = triggers
			.map((trigger) => `${trigger.id} --> ${trigger.robotId}`)
			.join('\n');
		const robotsToQueues = robots
			.reduce((acc, curr) => {
				const uniqueQueues = new Set<string>(curr.queues.map((queue) => queue.queueId));
				const robotQueues = Array.from(uniqueQueues).map((unique) => {
					return `${curr.id} --> ${unique}`;
				});
				acc.push(...robotQueues);
				return acc;
			}, [] as string[])
			.join('\n');
		const robotsToLibraries = Array.from(
			robots.reduce((acc, curr) => {
				curr.libraries.forEach((library) => {
					acc.add(`${curr.id} --> ${library}`);
				});

				return acc;
			}, new Set<string>())
		).join('\n');
		const robotsToTasks = Array.from(
			robots.reduce((acc, curr) => {
				curr.tasks.forEach((task) => {
					acc.add(`${curr.id} <-..-> ${task}`);
				});

				return acc;
			}, new Set<string>())
		).join('\n');
		return removeDashesFromUUIDs(
			mermaidTemplate
				.replace('{{QueueNodeDefinitions}}', queueNodes)
				.replace('{{TaskNodeDefinitions}}', taskNodes)
				.replace('{{RobotNodeDefinitions}}', robotNodes)
				.replace('{{LibraryNodeDefinitions}}', libraryNodes)
				.replace('{{TargetApplicationDefinitions}}', targetApplicationNodes)
				.replace('{{TriggerNodeDefinitions}}', triggerNodes)
				.replace(
					'{{Connections}}',
					libraryToApplicationConnections +
						'\n' +
						queuesToTriggers +
						'\n' +
						triggersToRobots +
						'\n' +
						robotsToQueues +
						'\n' +
						robotsToLibraries +
						'\n' +
						robotsToTasks
				)
		);
	}
	function removeDashesFromUUIDs(text: string) {
		// Regular expression to match UUIDs
		const uuidRegex =
			/\b[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\b/g;
		// Replace dashes in each UUID with an empty string
		return text.replace(uuidRegex, (uuid: string) => uuid.replace(/-/g, ''));
	}
	$inspect('solution', $solution);
	$inspect('mermaid', mermaidStr);
	// function getEmptyString(key: string) {
	// 	switch (key) {
	// 		case 'libraries':
	// 			return 'No libraries! Good! My automations are works of art, not to be recreated or reused by anyone...including me. I will sue.';
	// 		case 'robots':
	// 			return "No robots! Good! It was cheaper to get some folks from India anyways. (@HR - I have an Indian passport, its a joke, don't fire me)";
	// 		case 'queues':
	// 			return "No queues! Good! No one needs to know what I'm doing with all this data.";
	// 		case 'tasks':
	// 			return "No tasks! Good! We don't need no human input, we don't need no help from you.";
	// 		default:
	// 			return '';
	// 	}
	// }
</script>

<DesignEditor data={solution} columns={cols} />
<Tabs.Content value="view">
	<MermaidViewer value={mermaidStr} />
</Tabs.Content>
