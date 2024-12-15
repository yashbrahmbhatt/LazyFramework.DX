<script lang="ts">
	import { goto } from "$app/navigation";
	import FadeInOut from "$lib/composites/FadeInOut.svelte";
	import { getContext } from "svelte";
	let {data} = $props();

	setTimeout(() => {
		goto('/solution');
	}, 8000);

	
</script>
<div class="h-[95vh] w-full flex flex-col place-content-center space-y-4">
	<FadeInOut class='self-center -translate-y-12' delayIn={500} delayOut={2500}>
	  <h1 class="text-4xl font-bold text-center">Welcome</h1>
	</FadeInOut>
  
	<FadeInOut  class='self-center -translate-y-12'  delayIn={3000} delayOut={5000}>
	  <h1 class="text-2xl text-center">I hope this makes things easier</h1>
	</FadeInOut>
  
	<FadeInOut class='self-center -translate-y-12'  delayIn={5500} delayOut={7500}>
	  <h1 class="text-2xl text-center">Don't hurt yourself though :)</h1>
	</FadeInOut>
  </div>

<style>

</style>
