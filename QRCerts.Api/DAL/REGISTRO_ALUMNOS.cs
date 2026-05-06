// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.DAL.REGISTRO_ALUMNOS
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

#nullable enable
namespace QRCerts.Api.DAL
{
    public class REGISTRO_ALUMNOS : DALBase
    {
        public Guid id_alumno { get; set; }

        public Guid id_curso { get; set; }
        public string observaciones { get; set; }
        public int? MoodleUserId { get; set; }
        public int? MoodleCourseId { get; set; }


        public REGISTRO_ALUMNOS()
        {
            id_alumno = Guid.NewGuid();
            id_curso = Guid.NewGuid();
            observaciones = string.Empty;
            MoodleUserId = null;
            MoodleCourseId = null;
        }

        private static List<REGISTRO_ALUMNOS> mapeo(SqlDataReader dr)
        {
            List<REGISTRO_ALUMNOS> registroAlumnosList = new List<REGISTRO_ALUMNOS>();
            if (dr.HasRows)
            {
                int ordinal1 = dr.GetOrdinal("id_alumno");
                int ordinal2 = dr.GetOrdinal("id_curso");
                int ordinalMoodleUserId = -1;
                int ordinalMoodleCourseId = -1;
                int ordinalObservaciones = -1;
                try { ordinalMoodleUserId = dr.GetOrdinal("MoodleUserId"); } catch { }
                try { ordinalMoodleCourseId = dr.GetOrdinal("MoodleCourseId"); } catch { }
                try { ordinalObservaciones = dr.GetOrdinal("observaciones"); } catch { }
                while (dr.Read())
                {
                    REGISTRO_ALUMNOS registroAlumnos = new REGISTRO_ALUMNOS();
                    if (!dr.IsDBNull(ordinal1))
                        registroAlumnos.id_alumno = dr.GetGuid(ordinal1);
                    if (!dr.IsDBNull(ordinal2))
                        registroAlumnos.id_curso = dr.GetGuid(ordinal2);
                    if (ordinalMoodleUserId >= 0 && !dr.IsDBNull(ordinalMoodleUserId))
                        registroAlumnos.MoodleUserId = dr.GetInt32(ordinalMoodleUserId);
                    if (ordinalMoodleCourseId >= 0 && !dr.IsDBNull(ordinalMoodleCourseId))
                        registroAlumnos.MoodleCourseId = dr.GetInt32(ordinalMoodleCourseId);
                    if (ordinalObservaciones >= 0 && !dr.IsDBNull(ordinalObservaciones))
                        registroAlumnos.observaciones = dr.GetString(ordinalObservaciones);
                    registroAlumnosList.Add(registroAlumnos);
                }
            }
            return registroAlumnosList;
        }

        public static void Insert(REGISTRO_ALUMNOS obj)
        {
            try
            {
                string str =
                            @"INSERT INTO REGISTRO_ALUMNOS
                    (id_alumno, id_curso, observaciones, MoodleUserId, MoodleCourseId)
                    VALUES
                    (@id_alumno, @id_curso, @observaciones, @MoodleUserId, @MoodleCourseId)";
                using (SqlConnection connection = DALBase.GetConnection())
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = str.ToString();
                    command.Parameters.AddWithValue("@id_alumno", (object)obj.id_alumno);
                    command.Parameters.AddWithValue("@id_curso", (object)obj.id_curso);
                    command.Parameters.AddWithValue("@observaciones", (object)obj.observaciones ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MoodleUserId", obj.MoodleUserId.HasValue ? (object)obj.MoodleUserId.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@MoodleCourseId", obj.MoodleCourseId.HasValue ? (object)obj.MoodleCourseId.Value : DBNull.Value);
                    command.Connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Update(Guid id_alumno, Guid id_curso, string observaciones)
        {
            try
            {
                string str = @"UPDATE REGISTRO_ALUMNOS
                       SET observaciones=@observaciones
                       WHERE id_alumno=@id_alumno AND id_curso=@id_curso";
                using (SqlConnection connection = DALBase.GetConnection())
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = str;
                    command.Parameters.AddWithValue("@id_alumno", (object)id_alumno);
                    command.Parameters.AddWithValue("@id_curso", (object)id_curso);
                    command.Parameters.AddWithValue("@observaciones", (object)observaciones ?? DBNull.Value);
                    command.Connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void delete(Guid id_alumno, Guid id_curso)
        {
            try
            {
                string str = @"DELETE REGISTRO_ALUMNOS
                       WHERE id_alumno=@id_alumno AND id_curso=@id_curso";
                using (SqlConnection connection = DALBase.GetConnection())
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = str.ToString();
                    command.Parameters.AddWithValue("@id_alumno", (object)id_alumno);
                    command.Parameters.AddWithValue("@id_curso", (object)id_curso);
                    command.Connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static REGISTRO_ALUMNOS? GetByMoodleIds(int moodleUserId, int moodleCourseId)
        {
            try
            {
                string str = @"SELECT id_alumno, id_curso, observaciones, MoodleUserId, MoodleCourseId
                               FROM REGISTRO_ALUMNOS
                               WHERE MoodleUserId = @MoodleUserId AND MoodleCourseId = @MoodleCourseId";
                using (SqlConnection connection = DALBase.GetConnection())
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = str;
                    command.Parameters.AddWithValue("@MoodleUserId", moodleUserId);
                    command.Parameters.AddWithValue("@MoodleCourseId", moodleCourseId);
                    command.Connection.Open();
                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        var list = mapeo(dr);
                        return list.Count > 0 ? list[0] : null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
