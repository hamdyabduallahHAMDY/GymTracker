window.gymTrackerAlarm = {
    start: function () {

        const AudioContext =
            window.AudioContext || window.webkitAudioContext;

        if (!AudioContext)
            return;

        const context = new AudioContext();

        let count = 0;

        const beep = () => {

            const oscillator = context.createOscillator();
            const gain = context.createGain();

            oscillator.connect(gain);
            gain.connect(context.destination);

            oscillator.frequency.value = 900;
            oscillator.type = "sine";

            gain.gain.setValueAtTime(
                0.25,
                context.currentTime);

            oscillator.start();

            oscillator.stop(
                context.currentTime + 0.2);

            count++;

            if (count >= 10) {
                clearInterval(interval);

                setTimeout(() => {
                    context.close();
                }, 500);
            }
        };

        beep();

        const interval =
            setInterval(beep, 500);
    }
};