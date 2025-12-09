using UnityEngine;

public static class CubeTopology
{
    // 면 index 정의 (CubeMinesweeper.cs와 같아야함)
    // 0:Top, 1:Bottom, 2:Left, 3:Right, 4:Front, 5:Back
    // Face 0: Top    (y+)
    // Face 1: Bottom (y-)
    // Face 2: Left   (x-)
    // Face 3: Right  (x+)
    // Face 4: Front  (z+)
    // Face 5: Back   (z-)
    
    // (현재 면, 이동 방향) -> (도착 면, 회전 필요 여부 등)
    // 이웃 찾기 함수
    public static void GetNeighbor(int face, int x, int y, int dx, int dy, int gridSize, out int outFace, out int outX, out int outY)
    {
        // 1. 일단 평면이라고 가정하고 이동
        int nx = x + dx;
        int ny = y + dy;
        int max = gridSize - 1;

        // 2. 범위를 벗어나지 않았다면 같은 면
        if (nx >= 0 && nx <= max && ny >= 0 && ny <= max)
        {
            outFace = face;
            outX = nx;
            outY = ny;
            return;
        }

        // 3. 범위를 벗어났다면 -> 면을 이동해야함
        // 3d 위상 수학 구현.......... 
        
        // Front(4) 기준 예시:
        // 오른쪽(nx > max) -> Right(3)의 왼쪽(x=0)으로
        // 왼쪽(nx < 0) -> Left(2)의 오른쪽(x=max)으로
        // 위(ny > max) -> Top(0)의 아래(y=0)로
        
        // 이 규칙을 6개 면 모두에 대해 switch-case로 작성해야함
        
        outFace = face; // default value
        outX = Mathf.Clamp(nx, 0, max);
        outY = Mathf.Clamp(ny, 0, max);

        switch (face)
        {
            case 0: // Top (윗면)
                if (ny > max)      { outFace = 4; outX = nx; outY = max; }      // 위로 -> Front의 위쪽
                else if (ny < 0)   { outFace = 5; outX = max - nx; outY = max; }// 아래로 -> Back의 위쪽 (X반전)
                else if (nx < 0)   { outFace = 2; outX = max - ny; outY = max; }// 왼쪽 -> Left의 위쪽 (축 회전)
                else if (nx > max) { outFace = 3; outX = ny; outY = max; }      // 오른쪽 -> Right의 위쪽 (축 회전)
                break;

            case 1: // Bottom (아랫면)
                if (ny > max)      { outFace = 4; outX = nx; outY = 0; }        // 위로(Visual상 뒤쪽) -> Front의 아래
                else if (ny < 0)   { outFace = 5; outX = max - nx; outY = 0; }  // 아래로 -> Back의 아래 (X반전)
                else if (nx < 0)   { outFace = 2; outX = max - ny; outY = 0; }        // 왼쪽 -> Left의 아래
                else if (nx > max) { outFace = 3; outX = ny; outY = 0; }  // 오른쪽 -> Right의 아래
                break;

            case 2: // Left (왼면)
                if (ny > max)      { outFace = 0; outX = 0; outY = max - nx; }  // 위로 -> Top의 왼쪽
                else if (ny < 0)   { outFace = 1; outX = 0; outY = max - nx; }        // 아래로 -> Bottom의 왼쪽
                else if (nx < 0)   { outFace = 4; outX = 0; outY = ny; }      // 왼쪽 -> Front의 왼쪽
                else if (nx > max) { outFace = 5; outX = max; outY = ny; }        // 오른쪽 -> Back의 오른쪽
                break;

            case 3: // Right (오른면)
                if (ny > max)      { outFace = 0; outX = max; outY = nx; }      // 위로 -> Top의 오른쪽
                else if (ny < 0)   { outFace = 1; outX = max; outY = nx; }// 아래로 -> Bottom의 오른쪽
                else if (nx < 0)   { outFace = 5; outX = 0; outY = ny; }      // 왼쪽 -> Back의 왼쪽
                else if (nx > max) { outFace = 4; outX = max; outY = ny; }        // 오른쪽 -> Front의 오른쪽
                break;

            case 4: // Front (앞면)
                if (ny > max)      { outFace = 0; outX = max - nx; outY = max; }        // 위로 -> Top의 아래
                else if (ny < 0)   { outFace = 1; outX = nx; outY = max; }      // 아래로 -> Bottom의 위 (Visual 좌표계상 Z축 대칭 고려)
                else if (nx < 0)   { outFace = 2; outX = 0; outY = ny; }      // 왼쪽 -> Left의 왼쪽
                else if (nx > max) { outFace = 3; outX = max; outY = ny; }        // 오른쪽 -> Right의 오른쪽
                break;

            case 5: // Back (뒷면)
                if (ny > max)      { outFace = 0; outX = max - nx; outY = max; }// 위로 -> Top의 위
                else if (ny < 0)   { outFace = 1; outX = max - nx; outY = 0; }  // 아래로 -> Bottom의 아래
                else if (nx < 0)   { outFace = 3; outX = 0; outY = ny; }      // 왼쪽(Visual상 오른쪽) -> Right의 왼쪽
                else if (nx > max) { outFace = 2; outX = max; outY = ny; }        // 오른쪽 -> Left의 오른쪽
                break;
        }
    }
}

//이렇게 코드로 면맞추다간 정신병걸릴거같아서 디버그 스크립트를 따로 만듭니다. - 확인 결과 3과 5면이 문제가 있습니다. 이를 수정했습니다.
//또 수정하니 2와 5면이 문제가 있습니다. 이를 수정했습니다. -> 이를 또 수정하니? 1과 3면이 문제가 있습니다ㅋㅋㅋㅋㅋ..
//또 수정을 했는데? 이젠 0과 4면이 문제가 있습니다ㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋ...........