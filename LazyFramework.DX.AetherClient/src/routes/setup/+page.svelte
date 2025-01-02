<script lang="ts" module>
	export interface SetupStep {
		id: string;
		title: string;
		description: string;
		choices: SetupChoiceOption[];
	}

	export interface SetupChoiceOption {
		title: string;
		description: string;
		more?: string;
		action: () => void;
		next: string;
	}

	export interface Module {
        id: string;
        title: string;
        type: ModuleType;
        inputQueues: Queue[],
        outputQueues: Queue[],
        persistence: boolean,
    }

    export enum ModuleType {
        Dispatcher,
        Performer,
        Reporter,
    }

    export interface Queue {
        id: string;
        name: string;
        folder: string;
        description: string;
        schema: Record<string, any>;
    }

    export interface ActionCenterTask {
        id: string;
        title: string;
        description: string;
    }
</script>

<script lang="ts">
	import Button, { buttonVariants } from '$lib/components/ui/button/button.svelte';
	import * as Popover from '$lib/components/ui/popover';
	import Separator from '$lib/components/ui/separator/separator.svelte';
	import FadeInOut from '$lib/composites/FadeInOut.svelte';
	import type { PageData } from './$types';

	let choices: SetupStep[] = [
		{
			id: 'start',
			title: "Let's start with how work is created.",
			description: 'Choose how the bots will be assigned work.',
			choices: [
				{
					title: 'File',
					description:
						'The bots will need to retrieve their own work from a file and then populate a Queue.',
					more: 'Dispatcher required to read files and create queue items. Implied use of a performer. Templates will be filtered accordingly.',
					action: () => {},
					next: 'file'
				},
				{
					title: 'Application',
					description:
						'The bots will need to retrieve their own work from an application and then populate a Queue.',
					more: 'Dispatcher required to retrieve data from an application. Implies use of a performer and library. Templates will be filtered accordingly.',
					action: () => {},
					next: 'file'
				},
				{
					title: 'External',
					description:
						'The work will be assigned to the bots by other systems or applications populating a Queue.',
					more: 'Dispatcher not required. Templates will be filtered accordingly.',
					action: () => {},
					next: 'file'
				},
				{
					title: 'User',
					description: 'The bots will be assigned work when the user runs them.',
					more: 'Dispatcher not required. User will be prompted, if necessary, to provide the required input for the bots to run. Templates will be filtered accordingly.',
					action: () => {},
					next: 'file'
				},
				{
					title: 'Event',
					description: "The bots will be assigned work when an event occurs on a user's machine.",
					more: "Dispatcher not required. The bot will respond to events on the user's machine to perform work. Templates will be filtered accordingly.",
					action: () => {},
					next: 'file'
				}
			]
		}
	];

	let currentChoice = $state(choices[0]);
    let wizardData = $state({});
    function selectWorkCreation(choice: SetupChoiceOption) {

    }

	let { data }: { data: PageData } = $props();
</script>

<div class="flex h-[95vh] w-full flex-col place-content-center space-y-4">
	<FadeInOut class="" delayIn={500} delayOut={-1} inDirection="right" outDirection="left">
		<h1 class="py-2 text-center text-xl font-bold">
			Let's start with how work will be assigned to the bots.
		</h1>
		<div class="flex w-full flex-row justify-center gap-4">
			<div class="flex max-w-36 flex-col gap-4">
				<p>
					The bots will need to retrieve their own work from an excel file or application by
					populating its own Queue.
				</p>
				<div class="flex-auto"></div>
				<Popover.Root>
					<Popover.Trigger
						class={buttonVariants({ variant: 'ghost', class: 'text-primary/50 underline' })}
						>More</Popover.Trigger
					>
					<Popover.Content>
						<span>
							The bots <span class="font-bold italic">will</span> require a dispatcher entry point following
							the dispatcher-performer pattern.
						</span>
					</Popover.Content>
				</Popover.Root>
				<Button>Self</Button>
			</div>
			<Separator orientation="vertical" class="bg-foreground" />
			<div class="flex max-w-36 flex-col gap-4">
				<p>
					The work will be assigned to the bots by other systems or applications populating a Queue.
				</p>
				<div class="flex-auto"></div>
				<Popover.Root>
					<Popover.Trigger
						class={buttonVariants({ variant: 'ghost', class: 'text-primary/50 underline' })}
						>More</Popover.Trigger
					>
					<Popover.Content>
						<span>
							The bots <span class="font-bold italic">will not</span> require a dispatcher entry point
							following the dispatcher-performer pattern.
						</span>
					</Popover.Content>
				</Popover.Root>
				<Button>3rd Party</Button>
			</div>
			<Separator orientation="vertical" class="bg-foreground" />
			<div class="flex max-w-36 flex-col gap-4">
				<p>The bots will be assigned work when the user runs them.</p>
				<div class="flex-auto"></div>
				<Popover.Root>
					<Popover.Trigger
						class={buttonVariants({ variant: 'ghost', class: 'text-primary/50 underline' })}
						>More</Popover.Trigger
					>
					<Popover.Content>
						<span>
							The user will be prompted, if necessary, to provide the required input for the bots to
							run.
						</span>
					</Popover.Content>
				</Popover.Root>
				<Button>User</Button>
			</div>
			<Separator orientation="vertical" class="bg-foreground" />
			<div class="flex max-w-36 flex-col gap-4">
				<p>The bots will be assigned work when an event occurs on a user's machine.</p>
				<div class="flex-auto"></div>
				<Popover.Root>
					<Popover.Trigger
						class={buttonVariants({ variant: 'ghost', class: 'text-primary/50 underline' })}
						>More</Popover.Trigger
					>
					<Popover.Content>
						<span> The bot will respond to events on the user's machine to perform work. </span>
					</Popover.Content>
				</Popover.Root>
				<Button>Trigger</Button>
			</div>
		</div>
	</FadeInOut>
</div>
