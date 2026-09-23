public class NetworkTimer
{
    float timer;
    public float MinTimeBetweenTicks;
    int currentTick;
    public NetworkTimer(float tickRate)
    {
        MinTimeBetweenTicks = 1f / tickRate;
    }

    public void Update(float delta)
    {
        timer += delta;
    }

    public bool ShouldTick()
    {
        if(timer >= MinTimeBetweenTicks)
        {
            timer -= MinTimeBetweenTicks;
            currentTick++;
            return true;
        }
        return false;
    }
}
