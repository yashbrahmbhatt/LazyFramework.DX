<script lang="ts">
    import * as Drawer from '$lib/components/ui/drawer';
	import * as ToggleGroup from '$lib/components/ui/toggle-group';
	import * as Table from '$lib/components/ui/table';
	import { onMount } from 'svelte';
	import Button from '$lib/components/ui/button/button.svelte';
    import {X, ListRestart, FilterX} from 'lucide-svelte'
    import Input from '$lib/components/ui/input/input.svelte';
    let { open = $bindable() }: {
        open: boolean;
    } = $props();
    let logs: {
		Level: string;
		Message: string;
		Timestamp: string;
		Context: string;
	}[] = [];

	onMount(async () => {
		try {
			const ws = new WebSocket('ws://localhost:7999/');

			ws.addEventListener('open', () => {
				console.log('WebSocket connection established.');
			});

			ws.addEventListener('message', (event) => {
				const message = event.data;
				try {
					const newLogs = JSON.parse(message);
					logs = newLogs;
				} catch (error) {
					console.error('Error parsing message:', error);
				}
			});

			ws.addEventListener('close', () => {
				console.log('WebSocket connection closed.');
			});

			ws.addEventListener('error', (error) => {
				console.error('WebSocket error:', error);
			});
		} catch (error) {
			console.error('Error connecting to WebSocket:', error);
		}
	});
	var contexts = $derived(new Set(logs.map((log) => log.Context).sort()));
    var levels = $derived(new Set(logs.map((log) => log.Level).sort()));
    var selectedLevels: string[] = $state([]);
	var selectedContexts: string[] = $state([]);
	var filteredLogs = $derived(logs.filter((log) => selectedContexts.includes(log.Context) && 
        (searchString === '' || log.Message.includes(searchString))
    && (selectedLevels.length === 0 || selectedLevels.includes(log.Level.toString()))));
    
	function getLogLevel(index: number): string {
		switch (index) {
			case 0:
				return 'Debug';
			case 1:
				return 'Info';
			case 2:
				return 'Warning';
			case 3:
				return 'Error';
			default:
				return 'UNKNOWN';
		}
	}

	function getLogColor(index: number): string {
		switch (index) {
			case 0:
				return 'text-foreground/50';
			case 1:
				return 'text-foreground';
			case 2:
				return 'text-yellow-500';
			case 3:
				return 'text-red-500';
			default:
				return 'text-gray-500';
		}
	}

	function formatTimestamp(timestamp: string): string {
		var date = new Date(timestamp);
		return `[${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}:${date.getSeconds().toString().padStart(2, '0')}]`;
	}

    let searchString: string = $state('');

    function clearFilters() {
        selectedContexts = [];
        searchString = '';
    }
</script>

<Drawer.Root bind:open>
	<Drawer.Content class="h-full" >
		<div class="flex flex-row py-2 px-2 space-x-2 w-full place-items-center">
            <Input class='flex w-96 self-center ml-4' placeholder='Search' bind:value={searchString} />
            <Button variant='ghost' size='sm' on:click={() => selectedContexts = []}><FilterX /></Button>
                <ToggleGroup.Root  variant='outline' class='self-start flex place-content-center h-full pl-10 pr-4' type="multiple"  bind:value={selectedContexts}>
                    {#each Array.from(contexts) as context}
					<ToggleGroup.Item class='self-center' value={context} >{context}</ToggleGroup.Item>
                    {/each}
                </ToggleGroup.Root>
                <ToggleGroup.Root  variant='outline' class='self-start flex place-content-center h-full pl-10 pr-4' type="multiple"  bind:value={selectedLevels}>
                    {#each Array.from(levels) as level}
					<ToggleGroup.Item class='self-center' value={level} >{getLogLevel(parseInt(level))}</ToggleGroup.Item>
                    {/each}
                </ToggleGroup.Root>
                <div class='flex flex-auto'></div>
            <div class='place-self-end flex flex-row space-x-2 py-2 pr-4'>

                <Button variant='ghost' class="" on:click={clearFilters}><ListRestart /></Button>
                <Button variant='ghost' on:click={() => open = !open} ><X /></Button>
            </div>
		</div>
		<Table.Root>
			<Table.Header>
				<Table.Row>
					<Table.Head class="w-min text-center">Timestamp</Table.Head>
					{#if selectedContexts.length > 1}
						<Table.Head class='w-min text-center'>Level</Table.Head>
					{/if}
					<Table.Head class='w-min text-center'>Context</Table.Head>
					<Table.Head>Message</Table.Head>
				</Table.Row>
			</Table.Header>
			<Table.Body>
				{#each filteredLogs as log}
					<Table.Row class={`${getLogColor(parseInt(log.Level))}`}>
						<Table.Cell class='p-1 text-center w-min'>{formatTimestamp(log.Timestamp)}</Table.Cell>
						<Table.Cell class='p-1 text-center w-min'>{getLogLevel(parseInt(log.Level))}</Table.Cell>
						{#if selectedContexts.length > 1}
							<Table.Cell class='p-1 text-center w-min'>{log.Context}</Table.Cell>
						{/if}
						<Table.Cell >{log.Message}</Table.Cell>
					</Table.Row>
				{/each}
			</Table.Body>
		</Table.Root>
	</Drawer.Content>
</Drawer.Root>